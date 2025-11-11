
provider "azurerm" {
  features {}
}

variable "resource_group" {
  default = "kurtis-rg"
}

variable "location" {
  default = "East US"
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group
  location = var.location
}

resource "azurerm_sql_server" "sql" {
  name                         = "kurtis-sql-${random_id.server.hex}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = "sqladminuser"
  administrator_login_password = var.sql_admin_password
}

resource "random_id" "server" {
  byte_length = 4
}

resource "azurerm_sql_database" "db" {
  name                = "KurtisDb"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  server_name         = azurerm_sql_server.sql.name
  sku_name            = "GP_Gen5_2"
  max_size_gb         = 10
}

variable "sql_admin_password" {
  type = string
}
