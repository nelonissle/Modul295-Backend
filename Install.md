Step 1:
Set environment
SMTPKEY="gmail key token"
SMTPUSER="gmail address"

Step 2:
run ./Install.ps1

Step 3:
admin user mit pwd erzeugen

tests laufen lassen


for mongosh in container:
mongosh -u mongoadmin -p secret

Use database 
use Modul295Db

show users
db.Users.find().pretty()