#!/usr/bin/env bash
# Swaps the staging slot into production.
set -euo pipefail

RESOURCE_GROUP=${RESOURCE_GROUP:-rg-products-service}
app=$(az functionapp list --resource-group "$RESOURCE_GROUP" --query "[0].name" --output tsv)

az functionapp deployment slot swap \
  --resource-group "$RESOURCE_GROUP" --name "$app" \
  --slot staging --target-slot production --output none

echo "Swapped staging -> production for $app"
