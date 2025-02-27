# UWP 2.0 Environments

CSV files are generated every 15 minutes by connecting directly to the Kubernetes cluster for each environment and getting the current deployments. The CSV files include the deployment name, image tag, and replica count for each deployment.

### Environment Specific
- [Dev Environment](./csv/dev.csv)
- [QA Environment](./csv/qa.csv)
- [UAT Environment](./csv/uat.csv)
- [Perf Environment](./csv/npd.csv)
- [Prod Environment](./csv/prod.csv)

### All Environments Combined
- [All Environments](./csv/all-environments.csv)
