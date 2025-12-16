# SentinelStack — Execution Plan

This document outlines the execution plan for building **SentinelStack**, an open-source, audit-first monitoring platform designed for regulated environments such as healthcare.

This plan is intentionally focused on **signal over scope**: demonstrating systems thinking, architectural discipline, and production-grade engineering practices.

---

## Project Goals

- Build a credible, enterprise-grade backend platform
- Demonstrate audit-first and compliance-aware design
- Validate architecture through real implementation
- Produce a portfolio artifact suitable for senior/staff-level review

---

## Technology Stack

- **Language:** C#
- **Framework:** .NET 10 / ASP.NET Core Web API
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Cloud:** AWS
- **IaC:** Terraform
- **Workers:** Python (optional, future)
- **Docs:** Markdown + Mermaid
- **Repo:** Public GitHub

---

## Guiding Principles

- Human-in-the-loop at all times
- No silent or automatic remediation
- Append-only audit logs
- Explicit ownership of actions
- Prefer clarity over premature optimization

---

## 7-Day Execution Plan

### Day 1 — Foundation & Architecture (Completed)

**Objectives**
- Establish architectural clarity
- Define system boundaries
- Avoid premature implementation

**Deliverables**
- GitHub repository initialized
- README.md written and polished
- Architecture diagram (Mermaid)
- Domain model diagram (Mermaid)
- Incident lifecycle / data flow diagram (Mermaid)
- Portfolio images captured

---

### Day 2 — Project Scaffolding

**Objectives**
- Create a stable project skeleton
- Enable local development

**Tasks**
- Create ASP.NET Core Web API project
- Establish solution structure
- Configure appsettings
- Add health check endpoint
- Add logging baseline
- Commit scaffold

**Deliverables**
- Running API locally
- Clean initial commit history

---

### Day 3 — Core Domain Modeling

**Objectives**
- Implement foundational entities
- Align code with documented domain model

**Tasks**
- Implement Incident entity
- Implement AuditLog entity (append-only)
- Implement Service entity (minimal)
- Implement User entity (minimal)
- Configure EF Core DbContext
- Generate initial migration

**Deliverables**
- Database schema
- Verified domain integrity

---

### Day 4 — Vertical Slice: Incident Lifecycle

**Objectives**
- Prove architecture through behavior
- Implement a full auditable workflow

**Tasks**
- Create Incident endpoint
- Write AuditLog on incident creation
- Implement incident resolution endpoint
- Write AuditLog on resolution
- Enforce immutability of AuditLog
- Add basic validation

**Deliverables**
- Working end-to-end incident flow
- Fully auditable lifecycle

---

### Day 5 — Access Control & Authorization

**Objectives**
- Introduce controlled access
- Enforce least privilege

**Tasks**
- Define roles (Admin, Operator, Viewer)
- Apply role-based authorization
- Restrict incident resolution actions
- Audit all privileged actions

**Deliverables**
- RBAC enforced
- Authorization documented

---

### Day 6 — Infrastructure & Deployment Foundations

**Objectives**
- Demonstrate deployment maturity
- Enable repeatable environments

**Tasks**
- Define Terraform project structure
- Provision PostgreSQL (RDS)
- Define IAM roles (least privilege)
- Prepare deployment pipeline (conceptual)

**Deliverables**
- Infrastructure as Code committed
- Deployment approach documented

---

### Day 7 — Hardening & Polish

**Objectives**
- Improve robustness
- Prepare for review

**Tasks**
- Add basic error handling
- Add structured logging
- Add minimal tests
- Clean up documentation
- Review code for clarity

**Deliverables**
- Stable demo-ready platform
- Clean public repo

---

## What Is Explicitly Out of Scope (For Now)

- UI dashboards
- Automatic remediation
- Multi-region deployments
- Full compliance certifications
- Production healthcare usage

---

## Tracking & Workflow

- Track progress using this document
- Use GitHub commits as the source of truth
- Add GitHub Issues only if scope expands
- Avoid heavy process tools (Jira) for solo development

---

## Definition of Done

SentinelStack is considered complete for this phase when:

- Architecture diagrams match implementation
- Incident lifecycle is fully auditable
- Core principles are enforced in code
- Repo is understandable without explanation
- Portfolio accurately reflects the system

---

## Notes

This plan is designed to be realistic, disciplined, and reflective of how real healthcare platforms are built and evolved.
