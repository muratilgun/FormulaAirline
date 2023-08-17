![Rider](https://img.shields.io/badge/Rider-000000?style=for-the-badge&logo=Rider&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/Rabbitmq-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)

# .NET 7 ile RabbitMQ Kullanımı Örneği :rabbit2: :rabbit:
## FormulaAirline Projesi
Bu proje, .NET 7 kullanarak RabbitMQ mesaj kuyruğunu nasıl kullanabileceğinizi göstermek için oluşturulmuştur.

## Başlangıç

Öncelikle, RabbitMQ sunucusunun yüklü olduğundan emin olun. Eğer yüklü değilse, resmi [RabbitMQ indirme sayfası](https://www.rabbitmq.com/download.html)'ndan indirebilirsiniz.

## Kullanım

Bu proje, FormulaAirline havayolu şirketi uygulamasının bir parçasıdır. Bu README dosyası, projede RabbitMQ mesaj kuyruğunu nasıl entegre edeceğinizi açıklar.

### Kullanılan Model

Projede kullanılan `Booking` modeli aşağıdaki gibidir:

```csharp
public class Booking
{
    public int Id { get; set; }
    public string PassengerName { get; set; } = "";
    public string PassportNo { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public int Status { get; set; }
}
```
> `IMessageProducer` interfacedeki `SendMessage` metodu ve `MessageProducer` sınıfındaki bu metodun implementasyonunun açıklamaları:

### `IMessageProducer` Interface

Bu arayüz, RabbitMQ kuyruğuna ileti göndermek için bir yöntem belirtir. Bu yöntem, herhangi bir türde (`T`) bir mesaj alır ve bu mesajı kuyruğa gönderir.

#### `SendMessage<T>(T message)` Metodu

Bu yöntem, bir `T` türünde mesajı alır ve bu mesajı RabbitMQ kuyruğuna gönderir.

### `MessageProducer` Sınıfı

Bu sınıf, `IMessageProducer` arayüzünü uygular ve gerçek ileti gönderme işlevselliğini sağlar.

#### `SendMessage<T>(T message)` Metodu

Bu metot, bir `T` türünde mesajı alır ve bu mesajı RabbitMQ kuyruğuna gönderir. İşte metodun adım adım açıklamaları:

1. `ConnectionFactory` nesnesi oluşturulur ve RabbitMQ sunucusu ile ilgili bağlantı ayarları belirtilir.
2. Bağlantı oluşturulur (`using` bloğu içinde, böylece işlem bittiğinde otomatik olarak kapatılır).
3. Kanal oluşturulur (`using` bloğu içinde, böylece işlem bittiğinde otomatik olarak kapatılır).
4. Kuyruk adı "bookings" olarak belirtilir ve kuyruğun tanımlandığından emin olunur. `durable` özelliği `true` olarak ayarlanır, böylece kuyruk mesajlar için kalıcı olacaktır.
5. Mesaj bir JSON dizesine dönüştürülür (`JsonSerializer.Serialize(message)`).
6. JSON dizesi UTF-8 kodlamasıyla baytlara dönüştürülür.
7. `BasicPublish` metodu kullanılarak mesaj kuyruğa gönderilir. İlk parametrede boş dize verilerek varsayılan değişim kullanılır, ikinci parametrede kuyruk adı belirtilir ve üçüncü parametrede gönderilecek mesajın bayt dizisi verilir.

Bu adımlar, `SendMessage<T>` metoduyla RabbitMQ kuyruğuna ileti göndermek için izlenir. Bu metodu kullanarak projenizdeki `Booking` veya diğer türlerdeki nesneleri kuyruğa gönderebilirsiniz.



### `BookingsController` Sınıfı

Bu controller, `Booking` modelinin oluşturulmasını ve bir kuyruğa gönderilmesini sağlar.

#### Constructor ve Bağımlılıklar

Controller, `ILogger<BookingsController>` ve `IMessageProducer` bağımlılıklarını alır. `ILogger`, günlükleme işlevselliği sağlar ve `IMessageProducer` ise RabbitMQ kuyruğuna ileti göndermek için kullanılır.

#### In-Memory Veritabanı

Bu controller, `_bookings` adında bir in-memory veritabanı kullanır. Oluşturulan rezervasyonları bu veritabanında saklar.(Örnek proje olduğu için database işlemlerini kısa tutmak adına bu yolu izledim.:watch:)

#### `CreatingBooking` Metodu

Bu metot, bir HTTP POST isteği aldığında çağrılır. Gelen `Booking` nesnesini model olarak alır.

1. Gelen modelin geçerli olup olmadığını kontrol eder. Eğer geçerli değilse, hatalı istek (BadRequest) durumunu döner.
2. Geçerli bir model ise, `_bookings` in-memory veritabanına yeni rezervasyon ekler.
3. `_messageProducer` nesnesini kullanarak yeni rezervasyonu RabbitMQ kuyruğuna gönderir.
4. İşlem başarılıysa, istek durumu olarak "OK" döner.

`CreatingBooking` metodu, kullanıcıların yeni rezervasyonlar oluşturmasını ve bu rezervasyonları hem in-memory veritabanına kaydetmeyi hem de RabbitMQ kuyruğuna göndermeyi sağlar.


### Docker Compose Konfigürasyonu (`docker-compose.yml`)

```yaml
version: '3.8'
services:
  rabbitmq:
    container_name: "rabbitmq"
    image: rabbitmq:3.8-management-alpine
    environment:
        - RABBITMQ_DEFAULT_USER=user
        - RABBITMQ_DEFAULT_PASS=mypass
    ports:
        # rabbitmq instance
        - "5672:5672"
        # web interface
        - "15672:15672"
```

RabbitMQ konteynerını yapılandırma:

- `container_name`: Konteynerin adı, "rabbitmq" olarak ayarlanır.
- `image`: Kullanılacak Docker imajı belirtilir (`rabbitmq:3.8-management-alpine`). Bu imaj, RabbitMQ'nun 3.8 sürümünü ve yönetim arayüzünü içerir.
- `environment`: Çevresel değişkenler ayarlanır. `RABBITMQ_DEFAULT_USER` ve `RABBITMQ_DEFAULT_PASS`, RabbitMQ yönetim arayüzüne giriş yaparken kullanılacak kullanıcı adı ve parolayı belirtir.
- `ports`: Konteynerin hangi portlarını dış dünyaya açacağını belirtir. `5672` portu, RabbitMQ istemcileri tarafından kullanılırken, `15672` portu RabbitMQ yönetim arayüzü sunar.

Bu Docker Compose yapılandırması, RabbitMQ konteynerini başlatmak ve projenin RabbitMQ ile iletişim kurmasını sağlamak amacıyla kullanılıyor. Bu sayede proje Docker konteynerleri içerisinde çalıştırabilir ve RabbitMQ sunucusuna kolaylıkla erişim sağlanabilir.


### FormulaAirline.TicketProcessing Konsol Uygulaması

Bu konsol uygulaması, RabbitMQ kuyruğundan gelen rezervasyon bilgilerini işlemek üzere tasarlanmıştır.

```csharp
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Welcome to the ticketing service!");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "user",
    Password = "mypass",
    VirtualHost = "/"
};

var conn = factory.CreateConnection();
using var channel = conn.CreateModel();
channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false);
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, eventArgs) =>
{
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"New ticker processing is initiated for - {message}");
};
channel.BasicConsume(queue: "bookings", autoAck: true, consumer: consumer);

Console.ReadKey();
```

Bu konsol uygulaması, RabbitMQ kuyruğundan gelen rezervasyon bilgilerini işlemek üzere tasarlanmıştır. Temel özellikleri şunlardır:

- `ConnectionFactory` ile RabbitMQ sunucusuna bağlantı ayarları tanımlanır.
- Bir bağlantı oluşturulur ve kanal açılır.
- "bookings" adlı kuyruk tanımlanır ve varsa işlenecek mesajları beklemek üzere bir consumer(tüketici) oluşturulur.
- Mesajlar alındığında consumer(tüketici) tarafından belirtilen işlev (`Received` olayı) çalıştırılır. Bu işlevde, gelen mesajın içeriği okunarak işlenir.
- `BasicConsume` ile kuyruktan mesaj alınmaya başlanır.
- `Console.ReadKey()` ifadesi, uygulamanın çalışmasını durdurmak için klavyeden bir tuşa basılmasını bekler.
> Örnek Output : 
```
Welcome to the ticketing service!
New ticker processing is initiated for - "{\"Id\":0,\"PassengerName\":\"string\",\"PassportNo\":\"string\",\"From\":\"string\",\"To\":\"string\",\"Status\":0}"
```
