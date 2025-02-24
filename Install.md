Step 1:
Set environment
SMTPKEY="gmail key token"
SMTPUSER="gmail address"

Step 2:
run ./Install.ps1

Step 3:
admin user mit pwd erzeugen via TestDataInserter

tests laufen lassen


for mongosh in container:
mongosh -u mongoadmin -p secret

Use database 
use Modul295Db
show users
db.Users.find().pretty()


## Mongo only
docker compose up db -d

## Mongo connection string

To use Mongo DB in container (mymongo is defined in docker-compose.yml)
"ConnectionString": "mongodb://mongoadmin:secret@mymongo:27017",

To use Mongo DB on local pc
"ConnectionString": "mongodb://mongoadmin:secret@localhost:27017",