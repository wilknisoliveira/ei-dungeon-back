Run the following script in the Postgres to allow uuid-ossp extension:
```
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

The default user is:
	- Username: admin
	- Password: admin123

Please, for security change the password of the default username in the first application running.

Populate the database with some game info using the controller "/api/game/GameInfo". You can use an example at "Infrastructure/Utils/GameInfoSeeding.json". 