клонировать в рекдактор с ветви "master"

Запуск с Postman:
- Запускаем проект в реадкторе по протоколу HTTP
- Создаем новое рабочее пространство HTTP в Postman
- Body -> from-data -> Key:File -> загружаем свой файл
- Создаем Post запрос /api/ad-platforms/upload/ Вставляем после порта в url строку
В исполняемом файле увидим вывод файла
- Создаем Get запрос /api/ad-platforms/search?location=/ru/svrd (location= пишем адрес локации)
В исполняемом файле увидим вывод локаций

Запуск через браузер:
- Запускаем проект в редакторе
- F12 -> консоль
- Если файла нет, создадим его в скрипте (не забудьте поменять номер порта в localhost) СМОТРЕТЬ Скрипт для браузера без файла:
- Если файл есть (не забудьте поменять номер порта в localhost) СМОТРЕТЬ Скрипт для браузера с файлом:
- В исполняемом файле увидим вывод файла
- Создаем Get запрос. В url вставляем http://localhost:5033/api/ad-platforms/search?location=/ru/svrd (location= пишем адрес локации. не забываем поменять номер порта)
- В исполняемом файле увидим результат 

!я создавал внешний файл asd.txt и загружал его в Postman:
Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd 


