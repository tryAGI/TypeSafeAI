#!/usr/bin/env bash
set -euo pipefail

dotnet tool restore
curl --fail --silent --show-error --location \
  --output openapi.yaml \
  https://api.typesafe.ai/openapi.json
dotnet tool run autosdk generate openapi.yaml \
  --namespace TypeSafeAI.Generated \
  --clientClassName RawTypeSafeClient \
  --targetFramework net10.0 \
  --output Generated \
  --exclude-deprecated-operations \
  --security-scheme Http:Header:Bearer \
  --api-key-env-var TYPESAFE_API_KEY \
  --base-url https://api.typesafe.ai \
  --base-url-env-var TYPESAFE_BASE_URL \
  --compute-discriminators \
  --generate-raw-model-data \
  --generate-http-exception-hierarchy \
  --generate-retry-handler \
  --clean-stale-files
