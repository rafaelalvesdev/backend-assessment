#!/usr/bin/env bash
# Provisions the Products resources, applies the SQL scripts to both databases
# and publishes the function to the *staging* slot. Swap with infra/swap.sh.
# Usage: infra/deploy.sh   (requires az login and Docker; reads SQL_ADMIN_PASSWORD from infra/.secrets.env)
set -euo pipefail
cd "$(dirname "$0")/.."

RESOURCE_GROUP=${RESOURCE_GROUP:-rg-products-service}
LOCATION=${LOCATION:-centralus}
source infra/.secrets.env

az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

current_package() {
  local app slot_args=()
  app=$(az functionapp list --resource-group "$RESOURCE_GROUP" --query "[0].name" --output tsv)
  [ -z "$app" ] && return 0
  [ "$1" = staging ] && slot_args=(--slot staging)
  az functionapp config appsettings list --resource-group "$RESOURCE_GROUP" --name "$app" "${slot_args[@]}" \
    --query "[?name=='WEBSITE_RUN_FROM_PACKAGE'].value | [0]" --output tsv 2>/dev/null || true
}

outputs=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
    productionPackageUrl="$(current_package production)" \
    stagingPackageUrl="$(current_package staging)" \
  --query properties.outputs --output json)

app=$(jq -r .functionAppName.value <<<"$outputs")
sql_server=$(jq -r .sqlServerFqdn.value <<<"$outputs")
storage=$(jq -r .storageAccountName.value <<<"$outputs")
staging_url=$(jq -r .stagingUrl.value <<<"$outputs")

my_ip=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" --server "${sql_server%%.*}" \
  --name deploy-client --start-ip-address "$my_ip" --end-ip-address "$my_ip" --output none

sqlcmd() {
  docker run --rm --platform linux/amd64 -v "$PWD/sql:/sql:ro" mcr.microsoft.com/mssql/server:2022-latest \
    /opt/mssql-tools18/bin/sqlcmd -S "tcp:$sql_server,1433" -U productsadmin -P "$SQL_ADMIN_PASSWORD" -b -l 60 "$@"
}

for database in productsdb-staging productsdb; do
  echo "Applying SQL scripts to $database"
  for script in sql/0*.sql; do
    sqlcmd -d "$database" -i "/sql/$(basename "$script")"
  done

  objects=$(sqlcmd -d "$database" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.objects WHERE name IN ('Products', 'fn_GetProductsJson', 'usp_InsertProduct')" | tail -1)
  if [ "$objects" != "3" ]; then
    echo "Schema check failed for $database (found $objects of 3 objects)" >&2
    exit 1
  fi
done

rm -rf artifacts && dotnet publish src/Products.Functions -c Release -o artifacts/products
(cd artifacts/products && zip -qr ../products.zip .)

release="products-$(date -u +%Y%m%d%H%M%S)-$(git rev-parse --short HEAD).zip"
storage_key=$(az storage account keys list --resource-group "$RESOURCE_GROUP" --account-name "$storage" --query "[0].value" --output tsv)

az storage blob upload --account-name "$storage" --account-key "$storage_key" \
  --container-name function-releases --name "$release" --file artifacts/products.zip --output none

package_url=$(az storage blob generate-sas --account-name "$storage" --account-key "$storage_key" \
  --container-name function-releases --name "$release" --permissions r --https-only \
  --expiry "$(date -u -d '+1 year' +%Y-%m-%dT%H:%MZ 2>/dev/null || date -u -v+1y +%Y-%m-%dT%H:%MZ)" --full-uri --output tsv)

az functionapp config appsettings set --resource-group "$RESOURCE_GROUP" --name "$app" --slot staging \
  --settings WEBSITE_RUN_FROM_PACKAGE="$package_url" --output none

echo "Deployed to staging: $staging_url"
echo "Run the Postman 'Products' folder against azure-staging, then infra/swap.sh"
