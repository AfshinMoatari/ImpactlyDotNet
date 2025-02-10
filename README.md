
### Requirements

Microsoft .Net SDK
Microsoft .Net Runtime
Microsoft .Net Core 3.1 SDK
Microsoft .Net Core 3.1 Runtime


### Development
Run local dynamo environment 
```
docker stop dynamodb-local && docker-compose -f docker-compose-dynamo.yml up --detach & dynamodb-admin
```

### Setup
Create CodeCommit repository\
Create CodePipeline\
Create AWS BuildProject\
Create AWS Elastic Registry (docker registry)\
Create AWS Elastic Beanstalk project\

### AWS pull CodeCommit repository

Upload ssh public key to amazon
IAM -> Users -> User -> Security -> Public SSH keys

Update \~./ssh/config with
```
Host git-codecommit.*.amazonaws.com
	User <SSH-ID-KEY>
	IdentityFile ~/.ssh/id_rsa
```