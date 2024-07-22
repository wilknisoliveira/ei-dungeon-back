Run the following script in the Postgres to allow uuid-ossp extension:
```
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

Run the following script in the Postgres to set the default admin user to admin role:
```
INSERT INTO public."RoleEntityUserEntity"(
	"RolesId", "UsersId")
	VALUES ('440e090b-1245-4cfe-bb62-b22a676ab441', '7d9ff283-6174-40a6-a317-f32a4a0620d0');
```

The default user is:
	- Username: admin
	- Password: admin123

Please, for security change the password of the default username in the first application running.

