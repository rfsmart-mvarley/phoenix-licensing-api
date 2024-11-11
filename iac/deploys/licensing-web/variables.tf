variable "namespace" {
  type        = string
  description = "Namespace for the resources."
}

variable "region" {
  type        = string
  description = "AWS Region"
  default     = "us-east-1"
}

variable "app_name" {
  type        = string
  description = "Name of the application to be deployed to ECS"
}

variable "environment" {
  type        = string
  description = "ID element. Usually used for region e.g. 'uw2', 'us-west-2', OR role 'prod', 'staging', 'dev', 'UAT'"
}

variable "route_53_a_record" {
  type        = string
  description = "List of A record domains to create for the health check service"
}

variable "route_53_zone_name" {
  type        = string
  description = "Route53 zone name used for looking up and creating an `A` record for the health check service"
}

variable "app_image_tag" {
  type = string
}

variable "app_image" {
  type = string
}

variable "service_declaration" {
  type = object({
    name = string
    type = string
  })
  description = "Meta data about the service"
}

variable "api_gateway_path" {
  type        = string
  description = "API gateway path"
  default     = "licensing"
}
