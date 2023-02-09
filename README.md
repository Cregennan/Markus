<a name="document_top"></a>

![build](https://github.com/cregennan/markus/actions/workflows/dotnet-publish.yml/badge.svg)
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<br />
<div align="center">
  <!--
  <a href="https://github.com/Cregennan/Markus">
    <img src="https://sun9-40.userapi.com/impg/C-lp8-7sgsZgA7Cs446NSjbwOk36cnhwy6vhPg/Wk1S23QoHdI.jpg?size=1200x738&quality=96&sign=726dd49d5111605b4b17fa4daa6bc82d&type=album" alt="Logo" height="80">
  </a>-->

  <h1 align="center">Markus</h3>

  <p align="center">
    Пишите курсовые и дипломные работы по ГОСТу в формате Markdown
    <br />
    <a href="https://github.com/Cregennan/Markus/wiki"><strong>Документация</strong></a>
    <br />
    <a href="https://github.com/Cregennan/Markus/issues">Отчеты о багах</a>
    ·
    <a href="https://github.com/Cregennan/Markus/labels/enhancement">Предложения</a>
  </p>
</div>


## О проекте

<p align="center"><img src="https://sun9-48.userapi.com/impg/lCr0CN_7JuIVd7BE6yAbgVGwzs8yGoL8mesMaA/tXqgnd3Lcis.jpg?size=397x149&quality=96&sign=0b8ead6e71c8053d5cbba00373c6f1d6&type=album" /></p>

Когда нужно написать диплом, начинается битва с Word - картинка сместилась и утащила за собой весь текст. В одном определенном абзаце на странице 34, почему-то исчез абзацный отступ 1,25.<br/>
Данная проблема решается с помощью Markus. Вы пишете свою работу не отвлекаясь на оформление по ГОСТу, приложение будет делать это за вас. 
Вы пишете то, что хотите увидеть в вашем документе - Markus сам применит все нужные стили оформления.

<p align="right">(<a href="#document_top">Вверх</a>)</p>


### Собственные шаблоны
Вы можете использовать собственные шаблоны `.dotm`. Создайте пустой документ Word, подстройте внешний вид элементов страницы (стили шрифтов, отступы и т.д.) на свое усмотрение и сохраните файл с расширением `.dotm`. <br/> Перед созданием проекта переместите шаблон в папку будущего проекта и выберите его при создании.

#### Переопределение шаблонов
Также, вы можете назвать свой файл шаблона так-же как и выбранный вами системный файл шаблона. В этом случае использоваться будет тот шаблон, который находится в папке проекта.

### Похожие решения
Данное приложение было вдохновлено [Gostdown](https://gitlab.iaaras.ru/iaaras/gostdown). Оно отлично выполняет свои задачи, но сложнее в использовании и расчитано на большие требования к оформлению. 
#### Преимущества Markus:
- Его легко использовать. Документ верстается одной командой
- Не требуется установка Pandoc.
- Не требуется наличие Word в системе. Markus не использует COM объекты Word а почти напрямую записывает данные в .docx файл. Вы можете продолжить редактирование в любой удобной для вас программе поддерживающей документы Word.

<p align="right">(<a href="#document_top">Вверх</a>)</p>

## Использование
Скачайте архив с приложением [отсюда](https://github.com/Cregennan/Markus/releases/latest). 
Распакуйте в удобной папке, откройте ее в любом терминале.

### Команды
- `markus new <Название проекта>` - создает новый шаблонный проект в выбранной папке
- `markus build` - верстает проект с использованием выбранного шаблона
- `markus help` - помощь по командам


#### Дополнительно
Для того, чтобы запускать **Markus** из любой папки, добавьте путь к папке с `markus.exe` в `PATH`. [Подробнее](https://learn.microsoft.com/ru-ru/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14))


## Ограничения
На данный момент приложение находится на ранней стадии разработки и дает только базовые возможности по верстке документов. Со временем, будут реализованы большинство необходимых функций и не будет необходимости редактировать документ вручную.

<p align="right">(<a href="#document_top">Вверх</a>)</p>

## Дорожная карта

### Приложение

- [ ] Выбор папки проекта в командах
- [ ] Команда "edit" для изменения настроек существуюего проекта

### Верстка

- [x] Базовая верстка
  - [x] Выделения текста (жирный, наклонный шрифты)
  - [ ] Дополнительные варианты выделения (перечеркнутый, подчеркнутый итд.)
  - [x] Горизонтальная черта
- [x] Заголовки
  - [x] Выделения в заголовках
- [x] Списки (нумерованные и не нумерованные)
  - [x] Вложенные списки до 6 уровня (Word поддерживает до 9 уровней, CommonMark до 6)
  - [x] Вложенные списки до 9 уровня
  - [x] Выделения в списках
- [x] Ссылки
  - [ ] Внутренние ссылки
- [x] Изображения
  - [x] Поддержка интернет ссылок
  - [x] Поддержка ссылок на изображения в папке проекта
  - [x] Поддержка абсолютных ссылок на файл на диске
  - [x] Поддержка описаний изображений
- [ ] Код
  - [x] Поддержка кода внутри абзаца
  - [ ] Поддержка блоков кода
  - [ ] Поддержка подсветки синтаксиса
- [ ] Сборка из нескольких файлов
  - [ ] Поддержка разбиения файла на множество других
- [ ] Главная страница
  - [ ] Возможность добавления главной страницы. *Будет реализована только с помощью склеивания с другим документом Word*
- [ ] Ссылки на литературу
  - [ ] Единый стиль оформления ссылок на литературу



<p align="right">(<a href="#document_top">Вверх</a>)</p>

## Вклад в проект

Буду рад **любому** вкладу в разработку проекта.

Если вы хотите предложить свои изменения, создайте pull request. Также вы можете предложить свои идеи как `issue` с тэгом `enchancement`.

- Сделайте fork проекта
- Внесите свои изменения в отдельную ветку
- Создайте Pull Request

<p align="right">(<a href="#document_top">Вверх</a>)</p>

[markus-screenshot]: https://sun9-48.userapi.com/impg/lCr0CN_7JuIVd7BE6yAbgVGwzs8yGoL8mesMaA/tXqgnd3Lcis.jpg?size=397x149&quality=96&sign=0b8ead6e71c8053d5cbba00373c6f1d6&type=album
[contributors-shield]: https://img.shields.io/github/contributors/Cregennan/Markus.svg?style=plastic
[contributors-url]: https://github.com/Cregennan/Markus/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Cregennan/Markus.svg?style=plastic
[forks-url]: https://github.com/Cregennan/Markus/network/members
[stars-shield]: https://img.shields.io/github/stars/Cregennan/Markus.svg?style=plastic
[stars-url]: https://github.com/Cregennan/Markus/stargazers
[issues-shield]: https://img.shields.io/github/issues/Cregennan/Markus.svg?style=plastic
[issues-url]: https://github.com/Cregennan/Markus/issues
[license-shield]: https://img.shields.io/github/license/Cregennan/Markus.svg?style=plastic
[license-url]: https://github.com/Cregennan/Markus/blob/master/LICENSE.txt
