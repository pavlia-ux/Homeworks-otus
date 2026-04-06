INSERT INTO "ToDoUser"("TelegramUserId", "TelegramUserName")
	VALUES (111, 'Анастасия'),
	(222, 'Дмитрий');


INSERT INTO "ToDoList"("ListName", "UserId")
	VALUES ('Список домашних дел', 1),
	('Список ДЗ для учебы', 2);


INSERT INTO "ToDoItem"("UserId", "ItemName", "DeadLine", "ListId", "ToDoItemState")
	VALUES (1, 'Выбросить мусор', '06-04-2026', 1, 1),
	(1, 'Помыть полы', '06-04-2026', 1, 0),
	(2, 'Решить пробник по математике', '02-04-2026', 2, 1);