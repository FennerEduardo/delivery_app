-- Generated Seed SQL Fixtures for PostgreSQL / MySQL
-- Feature: User Authentication & Token Issuance

INSERT INTO users (id, email, password_hash, role, is_active, created_at)
VALUES (
  'f47ac10b-58cc-4372-a567-0e02b2c3d479',
  'dev@example.com',
  '$2b$12$eImiTXuWVxfM37uY4JANjO5E.y5bXm8A8fK0iE8zK4aM4iO2oU7pS', -- 'Pass123!' hashed with bcrypt cost 12
  'ADMIN',
  true,
  NOW()
) ON CONFLICT (email) DO NOTHING;
