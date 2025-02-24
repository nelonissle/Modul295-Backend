Step 1:
run ./Install.ps1

Step 2:
admin user mit pwd erzeugen

tests laufen lassen


for mongosh in container:
mongosh -u mongoadmin -p secret

Use database 
use Modul295Db

show users
db.Users.find().pretty()