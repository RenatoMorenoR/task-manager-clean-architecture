-- Seed data for demo user and initial tasks
-- Password hash for 'Demo1234!' using BCrypt (cost factor 12)
-- Hash: $2a$12$yhvI8MzvwrNxg.T/GNI/keOoy6gX7E/wJHE4qIXXMIWDoRZUXLXwi

INSERT INTO users (id, email, password_hash, name)
VALUES (
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 
    'demo@taskmanager.com', 
    '$2a$12$yhvI8MzvwrNxg.T/GNI/keOoy6gX7E/wJHE4qIXXMIWDoRZUXLXwi', 
    'Demo User'
) ON CONFLICT (email) DO NOTHING;

INSERT INTO tasks (user_id, title, description, status, due_date)
VALUES 
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Review Clean Architecture book', 'Read chapters 5-8 and take notes', 1, NOW() + INTERVAL '5 days'),
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Prepare technical interview presentation', 'Focus on Clean Architecture and TDD', 0, NOW() + INTERVAL '2 days'),
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Write unit tests for domain layer', '100% coverage target', 2, NOW() - INTERVAL '1 day'),
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Configure Docker environment', 'Multi-stage builds and compose', 2, NOW() - INTERVAL '2 days'),
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'Implement JWT authentication', 'Stateless and ownership checks', 0, NOW() + INTERVAL '3 days');
