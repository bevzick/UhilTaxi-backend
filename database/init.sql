-- UhilTaxi
-- MySQL 8.4+
-- Generated from the provided DBML schema.
--
-- Non-destructive by default:
--   CREATE DATABASE IF NOT EXISTS ...
--
-- For a full local reset, uncomment the next two lines:
-- DROP DATABASE IF EXISTS `uhiltaxi`;
-- CREATE DATABASE `uhiltaxi` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS `uhiltaxi`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `uhiltaxi`;

-- =========================================================
-- USERS
-- =========================================================

CREATE TABLE IF NOT EXISTS `users` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `role` ENUM('client', 'driver', 'admin') NOT NULL,
    `status` ENUM('active', 'blocked') NOT NULL DEFAULT 'active',
    `first_name` VARCHAR(50) NOT NULL,
    `last_name` VARCHAR(50) NOT NULL,
    `phone` VARCHAR(20) NOT NULL,
    `email` VARCHAR(255) NULL,
    `password_hash` VARCHAR(255) NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_users`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_users_phone`
        UNIQUE (`phone`),

    CONSTRAINT `uq_users_email`
        UNIQUE (`email`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `client_profiles` (
    `user_id` BIGINT NOT NULL,
    `birth_date` DATE NULL,

    CONSTRAINT `pk_client_profiles`
        PRIMARY KEY (`user_id`),

    CONSTRAINT `fk_client_profiles_user`
        FOREIGN KEY (`user_id`)
        REFERENCES `users` (`id`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `driver_profiles` (
    `user_id` BIGINT NOT NULL,
    `license_number` VARCHAR(20) NOT NULL,
    `hire_date` DATE NOT NULL,
    `rating_average` DECIMAL(3,2) NOT NULL DEFAULT 0,
    `rating_count` INT NOT NULL DEFAULT 0,

    CONSTRAINT `pk_driver_profiles`
        PRIMARY KEY (`user_id`),

    CONSTRAINT `uq_driver_profiles_license_number`
        UNIQUE (`license_number`),

    CONSTRAINT `fk_driver_profiles_user`
        FOREIGN KEY (`user_id`)
        REFERENCES `users` (`id`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `refresh_tokens` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT NOT NULL,
    `token_hash` VARCHAR(255) NOT NULL,
    `expires_at` DATETIME NOT NULL,
    `revoked_at` DATETIME NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_refresh_tokens`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_refresh_tokens_token_hash`
        UNIQUE (`token_hash`),

    CONSTRAINT `fk_refresh_tokens_user`
        FOREIGN KEY (`user_id`)
        REFERENCES `users` (`id`)
) ENGINE=InnoDB;


-- =========================================================
-- FLEET
-- =========================================================

CREATE TABLE IF NOT EXISTS `car_models` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `brand` VARCHAR(50) NOT NULL,
    `model_name` VARCHAR(50) NOT NULL,
    `category` ENUM('economy', 'standard', 'business', 'xl') NOT NULL,
    `fuel_type` ENUM('petrol', 'diesel', 'electric', 'hybrid') NOT NULL,
    `seat_count` SMALLINT NOT NULL DEFAULT 4,

    CONSTRAINT `pk_car_models`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_car_models_brand_model_fuel`
        UNIQUE (`brand`, `model_name`, `fuel_type`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `cars` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `model_id` BIGINT NOT NULL,
    `license_plate` VARCHAR(20) NOT NULL,
    `vin_code` VARCHAR(17) NOT NULL,
    `year` YEAR NOT NULL,
    `color` VARCHAR(30) NOT NULL,
    `status` ENUM('active', 'maintenance', 'out_of_service')
        NOT NULL DEFAULT 'active',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_cars`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_cars_license_plate`
        UNIQUE (`license_plate`),

    CONSTRAINT `uq_cars_vin_code`
        UNIQUE (`vin_code`),

    CONSTRAINT `fk_cars_model`
        FOREIGN KEY (`model_id`)
        REFERENCES `car_models` (`id`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `shifts` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `driver_id` BIGINT NOT NULL,
    `car_id` BIGINT NOT NULL,
    `start_time` DATETIME NOT NULL,
    `end_time` DATETIME NULL,
    `start_mileage` INT NOT NULL,
    `end_mileage` INT NULL,
    `status` ENUM('open', 'closed') NOT NULL DEFAULT 'open',

    CONSTRAINT `pk_shifts`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_shifts_driver`
        FOREIGN KEY (`driver_id`)
        REFERENCES `driver_profiles` (`user_id`),

    CONSTRAINT `fk_shifts_car`
        FOREIGN KEY (`car_id`)
        REFERENCES `cars` (`id`),

    INDEX `idx_shifts_driver_status` (`driver_id`, `status`),
    INDEX `idx_shifts_car_status` (`car_id`, `status`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `maintenance` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `car_id` BIGINT NOT NULL,
    `service_date` DATE NOT NULL,
    `description` TEXT NOT NULL,
    `cost` DECIMAL(10,2) NOT NULL,
    `mileage_at_service` INT NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_maintenance`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_maintenance_car`
        FOREIGN KEY (`car_id`)
        REFERENCES `cars` (`id`),

    INDEX `idx_maintenance_car_service_date`
        (`car_id`, `service_date`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `energy_logs` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `shift_id` BIGINT NOT NULL,
    `quantity` DECIMAL(10,2) NOT NULL,
    `unit` ENUM('liter', 'kwh') NOT NULL,
    `total_cost` DECIMAL(10,2) NOT NULL,
    `station_name` VARCHAR(100) NOT NULL,
    `odometer_km` INT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_energy_logs`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_energy_logs_shift`
        FOREIGN KEY (`shift_id`)
        REFERENCES `shifts` (`id`),

    INDEX `idx_energy_logs_shift_created_at`
        (`shift_id`, `created_at`)
) ENGINE=InnoDB;


-- =========================================================
-- TARIFFS AND PROMOCODES
-- =========================================================

CREATE TABLE IF NOT EXISTS `tariffs` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(50) NOT NULL,
    `service_class` ENUM('economy', 'standard', 'business', 'xl') NOT NULL,
    `base_fare` DECIMAL(10,2) NOT NULL,
    `rate_per_km` DECIMAL(10,2) NOT NULL,
    `rate_per_min` DECIMAL(10,2) NOT NULL,
    `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_tariffs`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_tariffs_name`
        UNIQUE (`name`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `promocodes` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `code` VARCHAR(20) NOT NULL,
    `discount_value` DECIMAL(10,2) NOT NULL,
    `discount_type` ENUM('fixed', 'percentage') NOT NULL,
    `expiry_date` DATE NOT NULL,
    `max_uses` INT NOT NULL,
    `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
    `min_order_amount` DECIMAL(10,2) NULL,
    `max_discount_amount` DECIMAL(10,2) NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_promocodes`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_promocodes_code`
        UNIQUE (`code`)
) ENGINE=InnoDB;


-- =========================================================
-- ORDERS
-- =========================================================

CREATE TABLE IF NOT EXISTS `orders` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `client_id` BIGINT NOT NULL,
    `tariff_id` BIGINT NOT NULL,
    `assigned_driver_id` BIGINT NULL,
    `promocode_id` BIGINT NULL,
    `status` ENUM(
        'pending',
        'accepted',
        'driver_arriving',
        'in_progress',
        'completed',
        'cancelled'
    ) NOT NULL DEFAULT 'pending',

    `pickup_address` VARCHAR(255) NOT NULL,
    `pickup_lat` DECIMAL(10,7) NOT NULL,
    `pickup_lng` DECIMAL(10,7) NOT NULL,
    `destination_address` VARCHAR(255) NOT NULL,
    `destination_lat` DECIMAL(10,7) NOT NULL,
    `destination_lng` DECIMAL(10,7) NOT NULL,

    `estimated_distance_km` DECIMAL(8,2) NULL,
    `estimated_duration_min` INT NULL,
    `estimated_fare` DECIMAL(10,2) NULL,

    `cancellation_reason` VARCHAR(500) NULL,
    `cancelled_by_user_id` BIGINT NULL,

    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `accepted_at` DATETIME NULL,
    `arrived_at` DATETIME NULL,
    `started_at` DATETIME NULL,
    `completed_at` DATETIME NULL,
    `cancelled_at` DATETIME NULL,

    CONSTRAINT `pk_orders`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_orders_client`
        FOREIGN KEY (`client_id`)
        REFERENCES `client_profiles` (`user_id`),

    CONSTRAINT `fk_orders_tariff`
        FOREIGN KEY (`tariff_id`)
        REFERENCES `tariffs` (`id`),

    CONSTRAINT `fk_orders_assigned_driver`
        FOREIGN KEY (`assigned_driver_id`)
        REFERENCES `driver_profiles` (`user_id`),

    CONSTRAINT `fk_orders_promocode`
        FOREIGN KEY (`promocode_id`)
        REFERENCES `promocodes` (`id`),

    CONSTRAINT `fk_orders_cancelled_by_user`
        FOREIGN KEY (`cancelled_by_user_id`)
        REFERENCES `users` (`id`),

    INDEX `idx_orders_client_created_at`
        (`client_id`, `created_at`),

    INDEX `idx_orders_status_created_at`
        (`status`, `created_at`),

    INDEX `idx_orders_assigned_driver_status`
        (`assigned_driver_id`, `status`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `promocode_usages` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `promocode_id` BIGINT NOT NULL,
    `client_id` BIGINT NOT NULL,
    `order_id` BIGINT NOT NULL,
    `used_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_promocode_usages`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_promocode_usages_order`
        UNIQUE (`order_id`),

    CONSTRAINT `fk_promocode_usages_promocode`
        FOREIGN KEY (`promocode_id`)
        REFERENCES `promocodes` (`id`),

    CONSTRAINT `fk_promocode_usages_client`
        FOREIGN KEY (`client_id`)
        REFERENCES `client_profiles` (`user_id`),

    CONSTRAINT `fk_promocode_usages_order`
        FOREIGN KEY (`order_id`)
        REFERENCES `orders` (`id`)
) ENGINE=InnoDB;


CREATE TABLE IF NOT EXISTS `order_status_history` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `order_id` BIGINT NOT NULL,
    `from_status` ENUM(
        'pending',
        'accepted',
        'driver_arriving',
        'in_progress',
        'completed',
        'cancelled'
    ) NULL,
    `to_status` ENUM(
        'pending',
        'accepted',
        'driver_arriving',
        'in_progress',
        'completed',
        'cancelled'
    ) NOT NULL,
    `changed_by_user_id` BIGINT NOT NULL,
    `reason` VARCHAR(500) NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_order_status_history`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_order_status_history_order`
        FOREIGN KEY (`order_id`)
        REFERENCES `orders` (`id`),

    CONSTRAINT `fk_order_status_history_changed_by_user`
        FOREIGN KEY (`changed_by_user_id`)
        REFERENCES `users` (`id`),

    INDEX `idx_order_status_history_order_created_at`
        (`order_id`, `created_at`)
) ENGINE=InnoDB;


-- =========================================================
-- TRIPS
-- =========================================================

CREATE TABLE IF NOT EXISTS `trips` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `order_id` BIGINT NOT NULL,
    `shift_id` BIGINT NOT NULL,
    `actual_start_time` DATETIME NOT NULL,
    `actual_end_time` DATETIME NULL,
    `distance_km` DECIMAL(8,2) NULL,
    `duration_min` INT NULL,

    `tariff_name` VARCHAR(50) NOT NULL,
    `base_fare` DECIMAL(10,2) NOT NULL,
    `rate_per_km` DECIMAL(10,2) NOT NULL,
    `rate_per_min` DECIMAL(10,2) NOT NULL,

    `discount_amount` DECIMAL(10,2) NOT NULL DEFAULT 0,
    `final_fare` DECIMAL(10,2) NULL,

    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_trips`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_trips_order`
        UNIQUE (`order_id`),

    CONSTRAINT `fk_trips_order`
        FOREIGN KEY (`order_id`)
        REFERENCES `orders` (`id`),

    CONSTRAINT `fk_trips_shift`
        FOREIGN KEY (`shift_id`)
        REFERENCES `shifts` (`id`),

    INDEX `idx_trips_shift`
        (`shift_id`),

    INDEX `idx_trips_actual_start_time`
        (`actual_start_time`)
) ENGINE=InnoDB;


-- =========================================================
-- PAYMENTS
-- =========================================================

CREATE TABLE IF NOT EXISTS `payments` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `trip_id` BIGINT NOT NULL,
    `amount` DECIMAL(10,2) NOT NULL,
    `currency` CHAR(3) NOT NULL DEFAULT 'UAH',
    `payment_method` ENUM('cash', 'card', 'wallet') NOT NULL,
    `payment_status` ENUM(
        'pending',
        'succeeded',
        'failed',
        'refunded'
    ) NOT NULL DEFAULT 'pending',
    `provider` VARCHAR(50) NULL,
    `transaction_id` VARCHAR(100) NULL,
    `idempotency_key` VARCHAR(100) NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_payments`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_payments_transaction_id`
        UNIQUE (`transaction_id`),

    CONSTRAINT `uq_payments_idempotency_key`
        UNIQUE (`idempotency_key`),

    CONSTRAINT `fk_payments_trip`
        FOREIGN KEY (`trip_id`)
        REFERENCES `trips` (`id`),

    INDEX `idx_payments_trip_status`
        (`trip_id`, `payment_status`),

    INDEX `idx_payments_created_at`
        (`created_at`)
) ENGINE=InnoDB;


-- =========================================================
-- REVIEWS
-- =========================================================

CREATE TABLE IF NOT EXISTS `reviews` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `trip_id` BIGINT NOT NULL,
    `reviewer_id` BIGINT NOT NULL,
    `target_user_id` BIGINT NOT NULL,
    `rating` TINYINT NOT NULL,
    `comment` TEXT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_reviews`
        PRIMARY KEY (`id`),

    CONSTRAINT `uq_reviews_trip_reviewer`
        UNIQUE (`trip_id`, `reviewer_id`),

    CONSTRAINT `fk_reviews_trip`
        FOREIGN KEY (`trip_id`)
        REFERENCES `trips` (`id`),

    CONSTRAINT `fk_reviews_reviewer`
        FOREIGN KEY (`reviewer_id`)
        REFERENCES `users` (`id`),

    CONSTRAINT `fk_reviews_target_user`
        FOREIGN KEY (`target_user_id`)
        REFERENCES `users` (`id`)
) ENGINE=InnoDB;


-- =========================================================
-- VIOLATIONS
-- =========================================================

CREATE TABLE IF NOT EXISTS `violations` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `driver_id` BIGINT NOT NULL,
    `created_by_admin_id` BIGINT NOT NULL,
    `description` TEXT NOT NULL,
    `fine_amount` DECIMAL(10,2) NOT NULL,
    `violation_date` DATETIME NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_violations`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_violations_driver`
        FOREIGN KEY (`driver_id`)
        REFERENCES `driver_profiles` (`user_id`),

    CONSTRAINT `fk_violations_created_by_admin`
        FOREIGN KEY (`created_by_admin_id`)
        REFERENCES `users` (`id`),

    INDEX `idx_violations_driver_date`
        (`driver_id`, `violation_date`)
) ENGINE=InnoDB;


-- =========================================================
-- AUDIT LOGS
-- =========================================================

CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT NOT NULL,
    `action` VARCHAR(100) NOT NULL,
    `entity_type` VARCHAR(50) NOT NULL,
    `entity_id` BIGINT NULL,
    `metadata` JSON NULL,
    `ip_address` VARCHAR(45) NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT `pk_audit_logs`
        PRIMARY KEY (`id`),

    CONSTRAINT `fk_audit_logs_user`
        FOREIGN KEY (`user_id`)
        REFERENCES `users` (`id`),

    INDEX `idx_audit_logs_user_created_at`
        (`user_id`, `created_at`)
) ENGINE=InnoDB;
