CREATE TABLE "ToDoUser"
(
    id SERIAL PRIMARY KEY,
	"ForeignId" UUID NOT NULL DEFAULT gen_random_uuid(),
	"TelegramUserId" INTEGER NOT NULL,
	"TelegramUserName" VARCHAR NOT NULL,
	"RegisteredAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "ToDoList"
(
    id SERIAL PRIMARY KEY,
	"ForeignId" UUID NOT NULL DEFAULT gen_random_uuid(),
	"ListName" VARCHAR NOT NULL,
	"UserId" INTEGER REFERENCES "ToDoUser"(id),
	"ListCreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "ToDoItem"
(
    id SERIAL PRIMARY KEY,
	"ForeignId" UUID NOT NULL DEFAULT gen_random_uuid(),
	"UserId" INTEGER REFERENCES "ToDoUser"(id),
	"ItemName" VARCHAR NOT NULL,
	"ItemCreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	"DeadLine" TIMESTAMP NOT NULL,
	"StateChangedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	"ToDoListId" INTEGER REFERENCES "ToDoList"(id),
	"ToDoItemState" INTEGER NOT NULL
);

CREATE INDEX "idx_ToDoList_UserId" ON "ToDoList"("UserId");
CREATE INDEX "idx_ToDoItem_UserId" ON "ToDoItem"("UserId");
CREATE INDEX "idx_ToDoItem_ToDoListId" ON "ToDoItem"("ToDoListId");
CREATE UNIQUE INDEX "UX_ToDoUser_TelegramUserId" ON "ToDoUser"("TelegramUserId");
CREATE UNIQUE INDEX "UX_ToDoUser_ForeignId" ON "ToDoUser"("ForeignId");
CREATE UNIQUE INDEX "UX_ToDoList_ForeignId" ON "ToDoList"("ForeignId");
CREATE UNIQUE INDEX "UX_ToDoItem_ForeignId" ON "ToDoItem"("ForeignId");