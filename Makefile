# =============================================
# TaskManager — Developer Commands
# =============================================

.PHONY: up down dev prod build test logs clean db-reset help

# ─── Docker ───────────────────────────────
up: ## Start all services (production mode)
	docker compose -f docker-compose.yml up -d
	@echo "✅ Services started"
	@echo "   API:      http://localhost:5001"
	@echo "   Swagger:  http://localhost:5001/swagger"
	@echo "   Frontend: http://localhost:3000"

dev: ## Start all services (development mode with hot reload)
	docker compose up -d
	@echo "✅ Dev services started"
	@echo "   API (hot reload): http://localhost:5001"
	@echo "   Frontend (HMR):   http://localhost:3000"

down: ## Stop all services
	docker compose down

build: ## Rebuild all images
	docker compose build --no-cache

prod: ## Run production compose (no override)
	docker compose -f docker-compose.yml up --build -d

logs: ## Tail logs from all services
	docker compose logs -f

logs-api: ## Tail API logs only
	docker compose logs -f api

logs-db: ## Tail DB logs only
	docker compose logs -f postgres

# ─── Database ─────────────────────────────
db-reset: ## Drop and recreate database with fresh seed data
	docker compose exec db psql -U taskmanager -d taskmanager_db \
		-c "DROP TABLE IF EXISTS tasks CASCADE; DROP TABLE IF EXISTS users CASCADE;"
	docker compose exec db psql -U taskmanager -d taskmanager_db \
		-f /docker-entrypoint-initdb.d/001_schema.sql
	docker compose exec db psql -U taskmanager -d taskmanager_db \
		-f /docker-entrypoint-initdb.d/002_seed.sql
	@echo "✅ Database reset complete"

db-shell: ## Open psql shell
	docker compose exec db psql -U taskmanager -d taskmanager_db

db-backup: ## Backup database to ./backups/
	mkdir -p backups
	docker compose exec db pg_dump -U taskmanager taskmanager_db \
		> backups/backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "✅ Backup created in ./backups/"

# ─── Testing ──────────────────────────────
test: ## Run all tests with coverage
	dotnet test backend/TaskManager.Tests/ \
		--collect:"XPlat Code Coverage" \
		--results-directory ./TestResults \
		--logger "console;verbosity=normal"

test-watch: ## Run tests in watch mode
	dotnet watch test --project backend/TaskManager.Tests/

test-coverage: test ## Generate HTML coverage report
	reportgenerator \
		-reports:"./TestResults/**/coverage.cobertura.xml" \
		-targetdir:"./TestResults/coverage-report" \
		-reporttypes:Html
	@echo "✅ Coverage report: ./TestResults/coverage-report/index.html"

# ─── Cleanup ──────────────────────────────
clean: ## Remove containers, volumes, and built images
	docker compose down -v --rmi local
	@echo "✅ Cleaned up containers, volumes, and images"

clean-all: clean ## Clean + remove node_modules and .NET build artifacts
	rm -rf frontend/node_modules frontend/dist
	find . -name "bin" -type d | xargs rm -rf
	find . -name "obj" -type d | xargs rm -rf
	@echo "✅ Full clean complete"

# ─── Help ─────────────────────────────────
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

.DEFAULT_GOAL := help
