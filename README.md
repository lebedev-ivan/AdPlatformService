Запуск с Postman:
- Запускаем проект в реадкторе
- Создаем новое рабочее пространство HTTP
- Body -> from-data -> Key:File -> загружаем свой файл
- Создаем Post запрос /api/ad-platforms/upload/ Вставляем после порта в url строку
В исполняемом файле увидим вывод файла
- Создаем Get запрос /api/ad-platforms/search?location=/ru/svrd (location= пишем адрес локации)
В исполняемом файле увидим вывод локаций

Запуск через браузер:
- Запускаем проект в редакторе
- F12 -> консоль

Если файла нет, создадим его в скрипте (не забудьте поменять номер порта в localhost):

const formData = new FormData();

// Создаем файл с данными
const fileContent = `Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd`;

const file = new File([fileContent], 'platforms.txt', { type: 'text/plain' });

// Добавляем файл в FormData
formData.append('file', file);

// Отправляем POST-запрос
fetch('http://localhost:5256/api/ad-platforms/upload', {
    method: 'POST',
    body: formData
})
.then(response => response.text())
.then(data => console.log(data))
.catch(error => console.error('Ошибка:', error));

Если файл есть (не забудьте поменять номер порта в localhost):

// Создаем input элемент для выбора файла
const input = document.createElement('input');
input.type = 'file';

// Обрабатываем выбор файла
input.onchange = (event) => {
    const file = event.target.files[0]; // Получаем выбранный файл
    const formData = new FormData();
    formData.append('file', file);

    // Отправляем POST-запрос
    fetch('http://localhost:5256/api/ad-platforms/upload', {
        method: 'POST',
        body: formData
    })
    .then(response => response.text())
    .then(data => console.log(data))
    .catch(error => console.error('Ошибка:', error));
};

// Запускаем выбор файла
input.click();

- В исполняемом файле увидим вывод файла
- Создаем Get запрос. В url вставляем http://localhost:5256/api/ad-platforms/search?location=/ru/svrd (location= пишем адрес локации. не забываем поменять номер порта)
- В исполняемом файле увидим результат 

!я создавал внешний файл asd.txt и загружал его в Postman:
Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd 


