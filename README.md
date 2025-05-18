# Slot manager (.NET Backend test)

## Start the project

1. Clone the repository.
2. Locate your terminal in the root directory of the project.
3. Execute `docker-compose up`

## Usage

This project can be tested in different ways, such as:
- Swagger: Going to `http://localhost:5292/swagger` will open the Swagger portal. There, you can manually test both endpoints (`Slots/availability/{date}` and `Slots/take`).
- HTTP file: In `SlotManager.API/Controllers/Slots` we can find the `SlotsController.http` file, that contains an example of the two mentioned endpoints.
It can be run in the IDE. In Rider, it can be run by clicking in the different green arrows found next to each endpoint.
