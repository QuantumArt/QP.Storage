# Докер-образ QP.Storage

## Назначение

Образ содержит приложение QP.Storage, которое устанавливается вместе с продуктом **QP8.CMS c поддержкой PostgreSQL**. Использование образа описано в [руководстве администратора QP8.CMS c поддержкой PostgreSQL](https://storage.qp.qsupport.ru/qa_official_site/images/downloads/qp8-pg-admin-man.pdf) (в разделе **Установка на Linux**).

## Репозитории

* [DockerHub](https://hub.docker.com/r/qpcms/qp-storage/tags): `qpcms/qp-storage`
* QA Harbor: `registry.quantumart.ru/qp8-cms/storage`

## История тегов (версий)

### 1.1.0.0

* переход на .NET 6, изменение манифестов
* если файл не найден, то при запросе его размера теперь выдаётся 0

### 1.0.0.10

* добавлен endpoint для получения размера файла

### 1.0.0.6

* базовая версия
