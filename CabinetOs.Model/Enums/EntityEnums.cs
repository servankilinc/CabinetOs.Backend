namespace CabinetOs.Model.Enums;

public static class EntityEnums
{
    /// <summary>
    /// Herhangi bir cihazın ağ/donanım durumu.
    /// Telemetri servisi tarafından güncellenir.
    /// </summary>
    public enum DeviceStatus
    {
        Offline = 0,
        Online = 1,
        Warning = 2,
        Critical = 3,
        Maintenance = 4
    }

    /// <summary>
    /// Diyagram üzerinde yer alan (pinli, kablolanabilir) cihazların kategorisi.
    /// DB'de tek Device tablosu TPH olarak tutulur, bu enum discriminator'dır.
    ///
    /// DİKKAT: Ağ üzerinden izlenen 3. parti cihazlar (kamera, UPS, switch)
    /// bu enum'da DEĞİLDİR — onlar <see cref="MonitoredDeviceType"/> kullanır.
    /// Böylece Toolbox'ta "kamera cihazı" gibi anlamsız bir Device üretilemez.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>Ana kontrolcü kart (Ethernet + RS485 master).</summary>
        ControlModule = 1,
        /// <summary>Dijital/analog giriş kartı (IN1..IN16).</summary>
        InputModule = 2,
        /// <summary>Röle çıkış kartı (OUT1..OUT15, NC/COM/NO).</summary>
        OutputModule = 3,
        /// <summary>LED gösterge kartı (LD1..LD8).</summary>
        LedModule = 4,
        /// <summary>Klemens / dağıtım bloğu — pasif, sadece kablo toplar.</summary>
        TerminalBlock = 5,
        /// <summary>Harici sensör (kabloyla bir giriş pinine bağlanan).</summary>
        Sensor = 6,
        /// <summary>Çevre birimi — siren, kilit, lamba, yazıcı, POS, barkod okuyucu vb.</summary>
        Peripheral = 7,
        /// <summary>Güç kaynağı / adaptör kartı.</summary>
        PowerSupply = 8,
        /// <summary>
        /// Panoya monteli ölçüm cihazı — enerji analizörü, voltmetre, ampermetre,
        /// akım trafosu. Kabin içinde RS485/Modbus hattında durur ve diyagramda
        /// klemensleriyle çizilir (ağdan SNMP ile izlenenler MonitoredDevice'tır).
        /// </summary>
        MeasurementDevice = 9,

        /// <summary>
        /// Kart okuyucu — geçiş kontrolünde kart ID'sini gönderen modül.
        ///
        /// Kendi ingest endpoint'i vardır çünkü taşıdığı veri bir kanal değeri
        /// değildir: <c>POST /api/scada/cardreader/{ExternalCode}</c> ile
        /// <c>{ "cardId": "A1B2C3D4" }</c> gelir. Kart ID'si IoChannel'a
        /// yazılmaz ve TelemetryRecord'a gitmez — o tablonun Value kolonu
        /// double'dır ve kart ID'si bir ölçüm değildir.
        /// </summary>
        CardReader = 10,

        /// <summary>
        /// Şebeke girişi — kabine dışarıdan gelen 220 AC beslemenin başladığı nokta.
        /// Pinleri L / N / PE'dir (referans diyagramdaki "ŞEBEKE").
        ///
        /// Bir kart değildir ama cihaz olmak ZORUNDADIR: <see cref="Connection"/>'ın
        /// iki ucu da zorunludur (<c>SourcePinId</c> / <c>TargetPinId</c> nullable
        /// değildir), yani dışarıdan gelen hat boşta uçlu çizilemez. Bu tip, o hattın
        /// kabin içindeki ilk ucunu taşır.
        ///
        /// <see cref="PowerSupply"/> ile karıştırılmamalıdır: o, 220 AC'yi DC'ye
        /// çeviren karttır; bu ise 220 AC'nin kabine giriş noktasıdır.
        /// </summary>
        Mains = 11,

        /// <summary>
        /// Sigorta / devre kesici — referans diyagramdaki "ŞEBEKE", "220V ÇIKIŞ"
        /// ve "LAMBA" şalterleri.
        ///
        /// Kesicinin DURUMU bu tipte değildir: izleniyorsa bağlı olduğu giriş
        /// kanalından okunur (siren ve kilit durumunun okunma şekliyle aynı).
        /// Tip cihazın NE OLDUĞUNU söyler, kanal HANGİ DURUMDA olduğunu — bu yüzden
        /// ayrı bir <c>BreakerState</c> enum'u yoktur.
        /// </summary>
        CircuitBreaker = 12
    }



    /// <summary>
    /// Pinin veri/enerji akış yönü.
    /// </summary>
    public enum PinDirection
    {
        Input = 0,
        Output = 1,
        Bidirectional = 2
    }

    /// <summary> React Flow bir handle'ı yerleştirmek için kenarı AÇIKÇA ister; <c>RelativeX/Y</c> tek başına yetmez </summary>
    public enum HandleSide
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }

    /// <summary>
    /// Pinin spesifik elektriksel fonksiyonu.
    /// String yerine enum kullanılarak tip güvenliği sağlanır.
    /// </summary>
    public enum PinFunction
    {
        /// <summary>Ortak uç (röle).</summary>
        COM = 0,
        /// <summary>Normally Open (röle).</summary>
        NO = 1,
        /// <summary>Normally Closed (röle).</summary>
        NC = 2,
        /// <summary>Pozitif besleme (+VCC).</summary>
        VCC = 3,
        /// <summary>Negatif / Toprak (GND).</summary>
        GND = 4,
        /// <summary>RS485 Data+ hattı.</summary>
        RS485_POS = 5,
        /// <summary>RS485 Data- hattı.</summary>
        RS485_NEG = 6,
        /// <summary>RJ45 Ethernet portu.</summary>
        RJ45 = 7,
        /// <summary>LED anot (+).</summary>
        LED_Anode = 8,
        /// <summary>LED katot (-).</summary>
        LED_Cathode = 9,
        /// <summary>Dijital giriş sinyali.</summary>
        Signal_In = 10,
        /// <summary>Dijital çıkış sinyali.</summary>
        Signal_Out = 11,
        /// <summary>Analog giriş.</summary>
        Analog_In = 12,
        /// <summary>Kuru kontak (Dry Contact).</summary>
        DryContact = 13,
        /// <summary>Faz (220 AC).</summary>
        Line_L = 14,
        /// <summary>Nötr (220 AC).</summary>
        Neutral_N = 15,
        /// <summary>Toprak / koruma hattı (PE).</summary>
        Earth_PE = 16,
        /// <summary>Genel amaçlı (özel tanımlı).</summary>
        General = 99
    }

    // NOT: "ModbusFunction" ve "RegisterDataType" enum'ları KALDIRILDI.
    // Fonksiyon kodu, register numarası, veri tipi ve word sırası artık
    // SCADA'nın içinde kalan taşıma detaylarıdır. Bu sistem sahayla Modbus
    // konuşmaz; SCADA ile HTTP üzerinden {modül, kanal, değer} alışverişi yapar.
    // Adresleme birimi IoChannel.ChannelNumber'dır.

    // ═══════════════════════════════════════════════════════════
    //  GERİLİM SEVİYESİ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Gerilim seviyesi — kablo bağlantı doğrulaması ve diyagram renklendirmesi için.
    /// Farklı seviyelerdeki pinlerin birbirine bağlanması engellenir.
    /// DC/AC ayrımı bu enum'un içinde kodludur (ayrı bir VoltageType'a gerek yoktur).
    /// </summary>
    public enum VoltageLevel
    {
        None = 0,
        DC_12V = 1,
        DC_24V = 2,
        AC_220V = 3,
        Signal_5V = 4,
        Data = 5
    }



    /// <summary>
    /// Kablo fiziksel türü.
    /// </summary>
    public enum WireType
    {
        Power = 0,
        Signal = 1,
        DataRS485 = 2,
        DataEthernet = 3,
        Relay = 4,
        Sensor = 5
    }

    /// <summary>
    /// Kablo çizim stili (UI).
    /// </summary>
    public enum LineStyle
    {
        Solid = 0,
        Dashed = 1,
        Dotted = 2
    }

    /// <summary>
    /// Kablonun canvas üzerinde çizim şekli.
    /// Referans diyagramdaki hatlar dik açılı (ortogonal) çizilmiştir.
    /// </summary>
    public enum EdgeRouting
    {
        /// <summary>Dik açılı kırılmalar (draw.io orthogonalEdgeStyle).</summary>
        Orthogonal = 0,
        /// <summary>Uçtan uca düz çizgi.</summary>
        Straight = 1,
        /// <summary>Yumuşak eğri (bezier).</summary>
        Curved = 2
    }



    /// <summary>
    /// Cihaz olmayan diyagram elemanlarının görsel biçimi.
    /// Referans diyagramdaki "BOZUK PARA KASA", "ŞEBEKE", "220V ÇIKIŞ"
    /// gibi serbest metin etiketleri bunlardır.
    ///
    /// NOT: "Group = 3" sabiti KALDIRILDI ve yerine bir süre kullanılan
    /// <c>DiagramGroup</c> entity'si de kaldırıldı. Canvas'ta gruplama diye bir
    /// kavram yoktur: bir cihazı sürüklerken pinleri ve kabloları zaten onunla
    /// gelir, çerçeve / katlama / toplu taşıma ise istenmemektedir.
    /// </summary>
    public enum AnnotationShape
    {
        /// <summary>Çerçevesiz düz metin (draw.io "text" stili).</summary>
        Text = 0,
        /// <summary>Çerçeveli kutu.</summary>
        Rectangle = 1,
        /// <summary>Not / açıklama balonu.</summary>
        Note = 2,
        /// <summary> Yön oku. </summary>
        Arrow = 3
    }

    /// <summary>
    /// Canvas arka plan deseni — React Flow &lt;Background variant&gt; karşılığı.
    /// Diyagramın içeriği değil, üzerinde çizildiği tualin görünümüdür.
    /// </summary>
    public enum BackgroundVariant
    {
        /// <summary>Desen yok — düz zemin rengi.</summary>
        None = 0,
        Dots = 1,
        Lines = 2,
        Cross = 3
    }



    /// <summary>
    /// SCADA'ya gönderilen kontrol isteğinin türü — <c>PayloadJson</c>'ın
    /// hangi şemayla okunacağını ve hangi endpoint'e gidileceğini belirler.
    ///
    /// Kumanda yolu güvenliğinin ilk halkasıdır: yetki kontrolü ("bu kullanıcı
    /// çıkış sürebilir mi?") ve doğrulama ("hedef kanalın Direction'ı Output mu?")
    /// bu alana bakarak yapılır. Serbest metin olsaydı yazım hatası bir komutu
    /// sessizce doğrulamanın dışına çıkarabilirdi.
    ///
    /// Yeni komut tipi eklemek MIGRATION GEREKTİRMEZ — kolon int'tir.
    /// </summary>
    public enum DeviceCommandType
    {
        /// <summary>
        /// Çıkış kanalını kalıcı olarak sürer (röle, LED).
        /// Payload: { "Value": 1 }
        ///
        /// <b>Bugün tek kumanda türü budur.</b> Darbe, değer yazma, modül reset ve
        /// senkronizasyon türleri kaldırıldı; her kumanda bir çıkış kanalını hedefler
        /// ve bir değer taşır. Bu yüzden <c>IoChannelId</c> ve <c>Value</c>
        /// koşulsuz zorunludur.
        /// </summary>
        SetOutput = 1
    }


    /// <summary>
    /// SCADA'ya gönderilen kumandanın sonucu.
    ///
    /// Kuyruk olmadığı için "Pending" ve "Cancelled" durumları YOKTUR:
    /// satır ancak istek gönderildikten sonra yazılır, gönderilmemiş bir
    /// komut kaydı hiç oluşmaz. AuditLog "kim istedi"yi, bu enum
    /// "SCADA kabul etti mi"yi kaydeder.
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>İstek gönderildi, cevap henüz işlenmedi (geçici ara durum).</summary>
        Sent = 1,
        /// <summary>SCADA komutu kabul ettiğini bildirdi (2xx).</summary>
        Succeeded = 2,
        /// <summary>SCADA hata döndürdü (4xx/5xx) — gövdesi ResultMessage'dadır.</summary>
        Failed = 3,
        /// <summary>Zaman aşımı veya bağlantı hatası — SCADA'ya hiç ulaşılamadı.</summary>
        NoResponse = 4
    }


    // VideoCodec enum'u KALDIRILDI. Sahada yalnızca H.264 kullanılıyor ve medya
    // geçidinde transcoding yapılmıyor; dolayısıyla seçilecek bir şey yoktu ve
    // hiçbir kod yolu bu alana bakmıyordu. Kodek artık bir veri alanı değil,
    // kurulum varsayımıdır — kameralar H.264'e ayarlanır.

    /// <summary>
    /// Hangi akışın izleneceği.
    ///
    /// Ayrı bir seçim olarak var, çünkü aksi halde arayüz her yerde ana akımı
    /// açar: 16 kameralık bir liste ekranında 16 adet 1080p akış demektir.
    /// Kamera üzerinde ikisi ayrı kanal numarasıdır (<c>MainStreamChannel</c> /
    /// <c>SubStreamChannel</c>) ve medya geçidinde ayrı birer yol olarak durur.
    /// </summary>
    public enum StreamProfile
    {
        /// <summary>Yüksek kalite — tam ekran / tek kamera görünümü.</summary>
        Main = 1,
        /// <summary>Düşük bant genişliği — liste, küçük önizleme.</summary>
        Sub = 2
    }

    /// <summary>
    /// Merkeze alınan görüntünün cinsi.
    ///
    /// Klip için ayrı bir "bitiş zamanı" alanı yoktur; bitiş
    /// <c>CapturedAtUtc + DurationSec</c>'tir.
    /// </summary>
    public enum CaptureType
    {
        /// <summary>Tek kare JPEG (~150 KB).</summary>
        Snapshot = 1,
        /// <summary>
        /// Kısa video klip. Medya geçidine kayıt açan geçici bir yol kurulup
        /// <c>DurationSec</c> kadar beklenerek üretilir; olay ÖNCESİNİ kapsamaz
        /// (öncesi için sürekli dönen bir kayıt tamponu gerekirdi, o da
        /// "7/24 kayıt yapılmaz" kararıyla çakışırdı).
        /// </summary>
        Clip = 2
    }

    /// <summary>
    /// Çekimin akıbeti.
    ///
    /// <see cref="Failed"/> de bir SATIR BIRAKIR: "olay anında görüntü YOK"
    /// bilgisinin kendisi delildir; satırı hiç yazmamak o bilgiyi siler.
    /// </summary>
    public enum CaptureStatus
    {
        /// <summary>İstek alındı, dosya henüz depoda değil.</summary>
        Pending = 1,
        /// <summary>Dosya depoda, indirilebilir.</summary>
        Available = 2,
        /// <summary>Alınamadı — sebebi <c>FailureReason</c>'dadır.</summary>
        Failed = 3
    }


    /// <summary>
    /// Kullanıcı izin türleri.
    ///
    /// <b>Bugün HİÇBİRİ ZORLANMIYOR.</b> Giriş sırasında üretilen token'a
    /// <c>permission</c> claim'i yazılır, ama sunucuda o claim'i okuyan tek bir
    /// kod yolu yoktur: uçlar yalnızca <c>[Authorize]</c> ile korunur, yani
    /// oturum açmış herkes hepsini çağırabilir. Arayüzdeki kumanda düğmeleri
    /// gizlenir — bu bir görünüm tercihidir, güvenlik sınırı DEĞİLDİR.
    /// Yetkilendirme <c>ROADMAP.md § Kapsam dışı</c>'nda planlı bir iş olarak durur.
    ///
    /// Değerler <b>0'dan başlar ve süreklidir</b> (0..9). <c>ViewDiagram = 0</c>
    /// olduğu için <c>Permission</c> lookup tablosunda
    /// <c>ValueGeneratedNever()</c> zorunludur; onsuz SQL Server IDENTITY üretmeye
    /// çalışır ve <c>HasData</c> seed'i reddedilir.
    ///
    /// Her üyenin <c>Permission</c> tablosunda bir satırı vardır
    /// (<c>AppDbContext</c> seed'i); tablodaki <c>Category</c> arayüzdeki izin
    /// matrisini gruplar. <b>Yeni üye eklerken seed satırı da eklenmelidir</b> —
    /// unutulursa <c>RolePermission</c> FK ihlali olarak çalışma anında patlar.
    /// </summary>
    public enum Permission
    {
        /// <summary>
        /// Kabin diyagramını açıp okuyabilir (kategori: <c>Diagram</c>).
        /// Sistemin en temel iznidir: diyagram bu üründe yalnızca bir çizim değil,
        /// ham sinyalin anlam kazandığı yerdir — "kanal 7 = 1" ancak burada
        /// "dış kapı hareket algıladı" olur.
        /// </summary>
        ViewDiagram = 0,

        /// <summary>
        /// Diyagramı düzenleyip kaydedebilir: cihaz bırakma/taşıma/silme, kablo
        /// çizme, not ekleme (kategori: <c>Diagram</c>).
        /// <see cref="ViewDiagram"/>'dan ayrıdır çünkü sahayı izleyen operatör ile
        /// kabini kuran teknisyen aynı kişi değildir; yanlış çizilmiş bir kablo
        /// sinyalin anlamını sessizce değiştirir.
        /// </summary>
        EditDiagram = 1,

        /// <summary>
        /// Çıkış kanalı sürebilir — röle, kilit, siren (kategori: <c>Control</c>).
        /// <b>Sahaya fiziksel etki eden tek izin budur;</b> diğerlerinin tamamı
        /// okuma ya da yapılandırmadır. <c>POST /api/Device/{id}/command</c>'in
        /// karşılığıdır ve zorlanmaya başlandığında ilk bağlanacak yer orasıdır.
        /// </summary>
        ControlOutput = 2,

        /// <summary>
        /// Alarmı görüp kabul edebilir (kategori: <c>Alarm</c>).
        /// <b>Karşılığı olan bir modül henüz YOK:</b> alarm tablosu, üreteci ve
        /// ucu yazılmadı. Üye, izin matrisi ileride alarm modülüyle birlikte
        /// geldiğinde numarası kaymasın diye şimdiden yerini tutuyor.
        /// </summary>
        AcknowledgeAlarm = 3,

        /// <summary>
        /// Kullanıcı ve rol yönetimi: hesap açma, rol atama, pasife alma
        /// (kategori: <c>Admin</c>).
        /// Kendi kendini büyüten bir izindir — bunu taşıyan bir hesap kendisine
        /// başka her izni verebilir.
        /// </summary>
        ManageUsers = 4,

        /// <summary>
        /// Sistem ve kabin yapılandırması: SCADA adresi, zaman aşımı, şablon
        /// kütüphanesi (kategori: <c>Admin</c>).
        /// <see cref="EditDiagram"/>'dan ayrıdır: o tek bir kabinin çizimini,
        /// bu kabinin sahayla NASIL konuştuğunu değiştirir.
        /// <c>Cabinet.ScadaBaseUrl</c>'i değiştirmek tüm telemetriyi ve kumandayı
        /// başka bir adrese yönlendirir.
        /// </summary>
        ConfigureSystem = 5,

        /// <summary>
        /// Kamera görüntüsü izleyebilir — anlık görüntü ve canlı yayın
        /// (kategori: <c>Diagram</c>).
        /// Kameralar diyagramda çizilmez ve verileri SCADA'dan gelmez, ama izin
        /// kategorisi <c>Diagram</c>'dır: operatör için ikisi de "kabinde ne
        /// oluyor" ekranının parçasıdır.
        /// <b>Kamera haberleşmesi henüz kodlanmadı</b>, dolayısıyla bu iznin
        /// koruyacağı bir uç de bugün yoktur.
        /// </summary>
        ViewCamera = 6,

        /// <summary>
        /// Listeleri ve geçmişi dışa aktarabilir — kanal olayları, kumanda
        /// geçmişi (kategori: <c>Data</c>).
        /// Okuma izinlerinden AYRI tutulur: ekranda sayfa sayfa görmek ile tüm
        /// geçmişi tek dosyada dışarı çıkarmak aynı risk değildir.
        /// Dışa aktarma ucu henüz yazılmadı.
        /// </summary>
        ExportData = 7,

        /// <summary>
        /// Otomasyon iş akışlarını tanımlar ve düzenler (kategori: <c>Admin</c>).
        /// <b>Automation modülü henüz kodlanmadı</b> — tasarımı
        /// <c>hibrit_uml.md</c>'de var, tablosu yok.
        /// Hassas izindir: bir iş akışı, insan eli değmeden çıkış sürebilir,
        /// yani <see cref="ControlOutput"/>'u dolaylı olarak kazandırır.
        /// </summary>
        ManageWorkflow = 8,

        /// <summary>
        /// Geçiş kartı tanımlama, yetkilendirme ve iptal etme
        /// (kategori: <c>Access</c>).
        /// Hassas izindir — kabin kapısını açan kimliği yönetir.
        /// <b>Access modülü henüz kodlanmadı.</b>
        /// <c>Permission</c> seed'ine satır EKLENMELİDİR; unutulursa
        /// <c>RolePermission</c> FK ihlali olarak çalışma anında patlar.
        /// </summary>
        ManageAccessCards = 9
    }

}
