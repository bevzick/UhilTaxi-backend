COMPOSE := docker compose

.DEFAULT_GOAL := help

.PHONY: help up start stop down restart build rebuild logs logs-api logs-db ps \
        api-shell db-shell clean reset config

help:
	@echo "TaxiPark Docker commands:"
	@echo "  make up         - build and start API + MySQL"
	@echo "  make start      - start existing containers"
	@echo "  make stop       - stop containers"
	@echo "  make down       - stop and remove containers"
	@echo "  make restart    - restart services"
	@echo "  make build      - build images"
	@echo "  make rebuild    - rebuild images without cache"
	@echo "  make logs       - follow all logs"
	@echo "  make logs-api   - follow API logs"
	@echo "  make logs-db    - follow MySQL logs"
	@echo "  make ps         - show service status"
	@echo "  make api-shell  - open shell inside API container"
	@echo "  make db-shell   - open MySQL client"
	@echo "  make config     - render resolved compose config"
	@echo "  make reset      - REMOVE containers AND MySQL volume"

up:
	$(COMPOSE) up -d --build

start:
	$(COMPOSE) start

stop:
	$(COMPOSE) stop

down:
	$(COMPOSE) down --remove-orphans

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
	$(COMPOSE) exec mysql sh -c \
		'mysql -u"$$MYSQL_USER" -p"$$MYSQL_PASSWORD" "$$MYSQL_DATABASE"'

config:
	$(COMPOSE) config

clean:
	$(COMPOSE) down --remove-orphans

reset:
	$(COMPOSE) down -v --remove-orphans
