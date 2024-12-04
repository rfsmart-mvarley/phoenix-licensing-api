with source_permissions (name, description, is_visible, scope) as (
values
(
    'control_plane_write'::text,
    'High level scope for Control Plane and SuperAdmin type access'::text,
    FALSE,
    'Global'
),
(
    'control_plane_read',
    'Read only access to the Control Plane',
    FALSE,
    'Global'
),
(
    'accounts_read',
    'Read only access to Users/Roles/Permissions',
    TRUE,
    'Organization'
),
(
    'accounts_write',
    'Write access to Users/Roles/Permissions',
    TRUE,
    'Organization'
),
(
    'workflows_read',
    'Read workflow configurations',
    TRUE,
    'Tenant'
),
(
    'workflows_write',
    'Write workflow configurations',
    TRUE,
    'Tenant'
),
(
    'read_receiving',
    'Read receiving transaction',
    TRUE,
    'Tenant'
),
(
    'write_receiving',
    'Write receiving transaction',
    TRUE,
    'Tenant'
),
(
    'license_read',
    'Read license information',
    TRUE,
    'Organization'
),
(
    'feature_read',
    'Read feature information',
    TRUE,
    'Organization'
),
(
    'featuregroup_read',
    'Write license information',
    TRUE,
    'Organization'
))

merge into accounts.permissions target
using source_permissions source
on source.name = target.name
when matched
then update
set 
	description = source.description,
    is_visible = source.is_visible,
    scope = source.scope,
	last_modified = CURRENT_TIMESTAMP,
	last_modified_by = 'system-migraton:phoenix-api'
when not matched then insert 
(
    id, 
    name, 
    description,
    is_visible,
    scope,
    created, 
    last_modified,
    created_by, 
    last_modified_by
)
values 
(   
    gen_random_uuid(), 
    source.name, 
    source.description,
    source.is_visible,
    source.scope,
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP, 
    'system-migraton:phoenix-api', 
    'system-migraton:phoenix-api'
);
