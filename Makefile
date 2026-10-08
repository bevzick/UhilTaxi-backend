COMPOSE := docker compose

.DEFAULT_GOAL := help

.PHONY: help up down stop start restart build rebuild logs logs-api logs-db ps db-shell api-shell config reset

help:
	@echo "UhilTaxi:"
	@echo "  make up        - build + start API and MySQL"
	@echo "  make down      - stop and remove containers"
	@echo "  make stop      - stop containers"
	@echo "  make start     - start containers"
	@echo "  make restart   - restart containers"
	@echo "  make build     - build API image"
	@echo "  make rebuild   - rebuild without cache"
	@echo "  make logs      - all logs"
	@echo "  make logs-api  - API logs"
	@echo "  make logs-db   - MySQL logs"
	@echo "  make ps        - container status"
	@echo "  make db-shell  - MySQL shell"
	@echo "  make api-shell - API shell"
	@echo "  make config    - validate docker-compose"
	@echo "  make reset     - remove containers and DB volume"

up:
	$(COMPOSE) up -d --build

down:
	$(COMPOSE) down --remove-orphans

stop:
	$(COMPOSE) stop

start:
	$(COMPOSE) start

restart:
	$(COMPOSE) restart

build:
	$(COMPOSE) build

rebuild:
	$(COMPOSE) build --no-cache

logs:
	$(COMPOSE) logs -f --tail=200

logs-api:
	$(COMPOSE) logs -f --tail=200 api

logs-db:
	$(COMPOSE) logs -f --tail=200 mysql

ps:
	$(COMPOSE) ps

api-shell:
	$(COMPOSE) exec api sh

db-shell:
	$(COMPOSE) exec mysql sh -c 'mysql -u"$$MYSQL_USER" -p"$$MYSQL_PASSWORD" "$$MYSQL_DATABASE"'

config:
	$(COMPOSE) config

reset:
	$(COMPOSE) down -v --remove-orphans