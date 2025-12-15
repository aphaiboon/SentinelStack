```
**Architecture Diagram**

The following ASCII diagram provides a high-level overview of SentinelStack's architecture:

```
+-------------------------------+
|    Client & Admin UI          |
| (Web Dashboard, Admin, Review)|
+---------------+---------------+
                |
                v
+---------------+---------------+
| ASP.NET Core Web API (.NET 8) |
|  - Authentication             |
|  - Incident Management        |
|  - Audit Logging              |
|  - Service Registry           |
+-------+-------+-------+-------+
        |       |
        v       v
+-------+   +---+------------------+
|       |   |                      |
|Background Workers                |
| - Incident Evaluation           |
| - Alerting                      |
| - Analytics / ML (Python)       |
+-------+-------------------------+
        |
        v
+---------------------------------+
|       PostgreSQL (RDS)          |
|  Users, Services, Incidents,    |
|  AuditLogs (append-only)        |
+---------------------------------+

    All core components (API, Workers, DB) are deployed within the AWS Cloud.
```
For a graphical diagram, please consult the repository or compatible Markdown viewers that support Mermaid.
```
