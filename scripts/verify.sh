#!/usr/bin/env bash
set -euo pipefail
dotnet restore backend/Ehsms.sln --locked-mode
dotnet list backend/Ehsms.sln package --vulnerable --include-transitive
dotnet build backend/Ehsms.sln --no-restore --configuration Release
dotnet test backend/Ehsms.sln --no-build --configuration Release
npm --prefix frontend ci
npm --prefix frontend audit --omit=dev --audit-level=high
npm --prefix frontend test -- --browsers=ChromeHeadless
npm --prefix frontend run build
if git ls-files | grep -E '(^|/)(node_modules|bin|obj)/|(^|/)\.env$'; then echo 'tracked generated/secret file detected' >&2; exit 1; fi
