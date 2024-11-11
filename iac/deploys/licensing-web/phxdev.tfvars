app_name           = "licensing-web"
app_image          = "ghcr.io/rf-smart-for-oraclecloud/licensing-web"
environment        = "phxdev"
namespace          = "phoenix"
route_53_a_record  = "licensing-api.phxdev.phoenix.rfsmart.com"
route_53_zone_name = "phxdev.phoenix.rfsmart.com"
service_declaration = {
  name = "licensing"
  type = "api"
}
