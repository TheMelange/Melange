# Melange Node

## Mounting the nodes test environment

```bash
	docker-compose up --build -d
	
```

## Executing one melange node at any port you want

```bash
	examples:

	docker-compose exec -e PORT=8888 melange-node bash -c "dotnet Melange.dll"
	docker-compose exec -e PORT=8887 melange-node bash -c "dotnet Melange.dll"
```
