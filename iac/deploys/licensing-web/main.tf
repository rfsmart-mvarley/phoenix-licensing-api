################################################################################
## defaults
################################################################################
terraform {
  required_version = "~> 1.4"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }

  backend "s3" {}
}


provider "aws" {
  region = var.region
}


module "tags" {
  source  = "sourcefuse/arc-tags/aws"
  version = "1.2.3"

  environment = var.environment
  project     = var.namespace

  extra_tags = {
    MonoRepo     = "True"
    MonoRepoPath = "iac/deploys/licensing-web"
    Repo         = "github.com/RF-SMART-for-OracleCloud/phoenix-licensing-api"
    map-migrated = "d-server-02nwr9q06bp0nt"
  }
}

module "ecs_app" {
  # tflint-ignore: terraform_module_pinned_source
  source = "github.com/RF-SMART-for-OracleCloud/phoenix-infra-ecs-web-app?ref=main"

  app_name               = var.app_name
  app_image              = var.app_image
  app_image_tag          = var.app_image_tag
  region                 = var.region
  service_declaration    = var.service_declaration
  listener_rule_priority = 7
  environment            = var.environment
  tags                   = module.tags.tags
  route_53_a_record      = var.route_53_a_record
  route_53_zone_name     = var.route_53_zone_name
  namespace              = var.namespace
  api_gateway_path       = var.api_gateway_path
}
