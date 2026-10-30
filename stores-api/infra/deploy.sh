#!/usr/bin/env bash
# Provisions the Stores API resources and deploys the current code.
# Usage: infra/deploy.sh   (requires az login; reads SQL_ADMIN_PASSWORD from infra/.secrets.env)
set -euo pipefail
cd "$(dirname "$0")/.."

RESOURCE_GROUP=${RESOURCE_GROUP:-rg-stores-api}
LOCATION=${LOCATION:-centralus}
PLAN_SKU=${PLAN_SKU:-F1}
source infra/.secrets.env

az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

outputs=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters sqlAdminPassword="$SQL_ADMIN_PASSWORD" planSku="$PLAN_SKU" \
  --query properties.outputs --output json)

app=$(jq -r .webAppName.value <<<"$outputs")
url=$(jq -r .url.value <<<"$outputs")

rm -rf artifacts && dotnet publish src/Stores.Api -c Release -o artifacts/stores-api
(cd artifacts/stores-api && zip -qr ../stores-api.zip .)

az webapp deploy --resource-group "$RESOURCE_GROUP" --name "$app" --src-path artifacts/stores-api.zip --type zip --output none

echo "Waiting for $url/health ..."
for _ in $(seq 1 30); do
  curl -sf "$url/health" >/dev/null && { echo "Stores API is up: $url"; exit 0; }
  sleep 10
done
echo "Health check did not pass yet; check the app logs." >&2
exit 1
