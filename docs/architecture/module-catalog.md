# Module catalog and ownership
| Module | Owner | Current artifact | Future responsibility |
|---|---|---|---|
| BuildingBlocks | Architecture | assembly marker | approved primitives only; never modules |
| SaaS | Platform team | four boundary assemblies | tenant/subscription/quota |
| Organisation | Organisation team | four boundary assemblies | hierarchy/people |
| Identity | IAM team | four boundary assemblies | identity/membership/access |
| Platform | Platform team | four boundary assemblies | records/workflow/evidence/audit/outbox |

Each module owns Domain, Application, Infrastructure and Contracts. Domain cannot point inward to Application/Infrastructure/API; Application cannot point to Infrastructure; only Contracts may cross module boundaries.
