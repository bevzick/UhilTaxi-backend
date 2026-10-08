-- Run once against an existing UhilTaxi database if the promocodes table is absent.
-- This leaves existing users/tariffs and MySQL Docker volume untouched.
USE `uhiltaxi`;
CREATE TABLE IF NOT EXISTS `promocodes` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(20) NOT NULL,
  `discount_value` DECIMAL(10,2) NOT NULL,
  `discount_type` ENUM('fixed','percentage') NOT NULL,
  `expiry_date` DATE NOT NULL,
  `max_uses` INT NOT NULL,
  `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
  `min_order_amount` DECIMAL(10,2) NULL,
  `max_discount_amount` DECIMAL(10,2) NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT `pk_promocodes` PRIMARY KEY (`id`),
  CONSTRAINT `uq_promocodes_code` UNIQUE (`code`)
) ENGINE=InnoDB;
