# phoenix-api App Deploy

## Overview

Deploys licensing-web to an ECS cluster.

<!-- BEGINNING OF PRE-COMMIT-TERRAFORM DOCS HOOK -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | ~> 1.4 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 4.0 |

## Providers

No providers.

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_ecs_app"></a> [ecs\_app](#module\_ecs\_app) | github.com/RF-SMART-for-OracleCloud/phoenix-infra-ecs-web-app | main |
| <a name="module_tags"></a> [tags](#module\_tags) | sourcefuse/arc-tags/aws | 1.2.3 |

## Resources

No resources.

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_app_image"></a> [app\_image](#input\_app\_image) | n/a | `string` | n/a | yes |
| <a name="input_app_image_tag"></a> [app\_image\_tag](#input\_app\_image\_tag) | n/a | `string` | n/a | yes |
| <a name="input_app_name"></a> [app\_name](#input\_app\_name) | Name of the application to be deployed to ECS | `string` | n/a | yes |
| <a name="input_environment"></a> [environment](#input\_environment) | ID element. Usually used for region e.g. 'uw2', 'us-west-2', OR role 'prod', 'staging', 'dev', 'UAT' | `string` | n/a | yes |
| <a name="input_namespace"></a> [namespace](#input\_namespace) | Namespace for the resources. | `string` | n/a | yes |
| <a name="input_region"></a> [region](#input\_region) | AWS Region | `string` | `"us-east-1"` | no |
| <a name="input_route_53_a_record"></a> [route\_53\_a\_record](#input\_route\_53\_a\_record) | List of A record domains to create for the health check service | `string` | n/a | yes |
| <a name="input_route_53_zone_name"></a> [route\_53\_zone\_name](#input\_route\_53\_zone\_name) | Route53 zone name used for looking up and creating an `A` record for the health check service | `string` | n/a | yes |
| <a name="input_service_declaration"></a> [service\_declaration](#input\_service\_declaration) | Meta data about the service | <pre>object({<br>    name = string<br>    type = string<br>  })</pre> | n/a | yes |

## Outputs

No outputs.
<!-- END OF PRE-COMMIT-TERRAFORM DOCS HOOK -->
<!-- BEGIN_TF_DOCS -->
<!-- END_TF_DOCS -->
