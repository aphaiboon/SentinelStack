flowchart LR
    Client[Client & Admin UI\n(Web Dashboard,\nAdmin & Reviewers)]

    API[ASP.NET Core Web API (.NET 8)\n\nAuthentication\nIncident Management\nAudit Logging\nService Registry]

    Workers[Background Workers\n\nIncident Evaluation\nAlerting\nAnalytics / ML (Python)]

    DB[(PostgreSQL\n\nUsers\nServices\nIncidents\nAuditLogs (append-only))]

    Client --> API
    API --> DB
    API --> Workers
    Workers --> DB

    subgraph Cloud[AWS Cloud]
        API
        Workers
        DB
    end
