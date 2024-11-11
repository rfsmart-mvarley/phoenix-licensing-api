app_name           = "TEMP-LATE-web"
app_image          = "ghcr.io/rf-smart-for-oraclecloud/TEMP-LATE-web"
environment        = "phxdev"
namespace          = "phoenix"
route_53_a_record  = "TEMP-LATE-api.phxdev.phoenix.rfsmart.com"
route_53_zone_name = "phxdev.phoenix.rfsmart.com"
service_declaration = {
  name = "TEMP-LATE"
  type = "api"
}
