-- SentinelStack Database Initialization Script
-- This script runs on first container startup

-- Create schemas
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create enum types
DO $$ BEGIN
    CREATE TYPE incident_status AS ENUM ('open', 'acknowledged', 'investigating', 'resolved', 'closed');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE incident_severity AS ENUM ('critical', 'high', 'medium', 'low', 'info');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE user_role AS ENUM ('viewer', 'responder', 'manager', 'admin', 'platform_admin');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE audit_action AS ENUM (
        'created', 'updated', 'deleted', 'accessed', 'login', 'login_failed',
        'logout', 'password_changed', 'role_changed', 'exported'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Grant permissions to hangfire schema
GRANT ALL ON SCHEMA hangfire TO sentinelstack;

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'SentinelStack database initialized successfully';
END $$;
