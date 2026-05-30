using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class VentaComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = default!;

    private VentaPreviewViewModel? Preview { get; set; }
    private PosCategoryNode? CurrentNode { get; set; }
    private List<PosCategoryNode> SelectedPath { get; } = [];
    private List<OrderLineItem> OrderItems { get; set; } = [];

    private IReadOnlyList<PosCategoryNode> VisibleCategories =>
        CurrentNode is null
            ? Preview?.RootCategories ?? []
            : CurrentNode.Children;

    private IReadOnlyList<PosProduct> VisibleProducts =>
        ShowingProducts && CurrentNode is not null
            ? CurrentNode.Products
            : [];

    private bool ShowingProducts =>
        CurrentNode is not null && CurrentNode.Children.Count == 0;

    private decimal Subtotal => OrderItems.Sum(item => item.Total);
    private decimal DiscountAmount => Preview?.DiscountAmount ?? 0m;
    private decimal ServiceCharge => Preview?.ServiceCharge ?? 0m;
    private decimal GrandTotal => Subtotal - DiscountAmount + ServiceCharge;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Preview = await LoadPreviewAsync();
        OrderItems = Preview.InitialOrderItems.ToList();
    }

    private Task<VentaPreviewViewModel> LoadPreviewAsync()
    {
        // Temporary visual data for the POS screen.
        // Replace this later with real categories, children and products from your API.
        return Task.FromResult(BuildPreview());
    }

    private static VentaPreviewViewModel BuildPreview()
    {
        return new VentaPreviewViewModel
        {
            BrandSubtitle = "Club de Billar",
            ShiftLabel = "Turno Noche",
            ShiftStatus = "En curso",
            ShiftTimeLabel = "Desde 18:00",
            CashierName = "Nahuel Zalazar",
            CashierRole = "Cajero",
            SearchPlaceholder = "Buscar productos, categorias o clientes...",
            CustomerSearchPlaceholder = "Buscar cliente...",
            DiscountAmount = 0m,
            ServiceCharge = 0m,
            PaymentMethods =
            [
                new PaymentMethod("Pagar", "bi bi-cash-stack", true),
            ],
            RootCategories =
            [
                new PosCategoryNode(
                    "bebidas",
                    "Bebidas",
                    "Gaseosas, cervezas y energizantes para venta rapida.",
                    "bi bi-cup-straw",
                    "tone-success",
                    "Barra fria",
                    Children:
                    [
                        new PosCategoryNode(
                            "gaseosas",
                            "Gaseosas",
                            "Linea fria personal y formatos para compartir.",
                            "bi bi-droplet-half",
                            "tone-primary",
                            "Clasicos",
                            Children:
                            [
                                new PosCategoryNode(
                                    "cola",
                                    "Linea Cola",
                                    "Sabores de alta rotacion para mostrador.",
                                    "bi bi-circle-fill",
                                    "tone-danger",
                                    "Top ventas",
                                    Products:
                                    [
                                        new PosProduct("coca-350", "Coca Cola 350 ml", "Lata fria individual.", 12m, "Bebidas", "bi bi-cup-straw", "tone-danger", "Lata fria", ["Cola", "Individual"]),
                                        new PosProduct("coca-500", "Coca Cola 500 ml", "Botella de salida rapida.", 16m, "Bebidas", "bi bi-bottle-soda", "tone-primary", "Botella", ["Cola", "500 ml"]),
                                        new PosProduct("coca-zero", "Coca Zero 500 ml", "Version sin azucar.", 16m, "Bebidas", "bi bi-bottle-soda", "tone-success", "Sin azucar", ["Zero", "Fria"])
                                    ]),
                                new PosCategoryNode(
                                    "sabores",
                                    "Sabores",
                                    "Naranja, lima limon y agua mineral.",
                                    "bi bi-sun-fill",
                                    "tone-warning",
                                    "Refrescantes",
                                    Products:
                                    [
                                        new PosProduct("fanta-500", "Fanta 500 ml", "Sabor naranja para barra.", 16m, "Bebidas", "bi bi-bottle-soda", "tone-warning", "Naranja", ["500 ml", "Fria"]),
                                        new PosProduct("sprite-500", "Sprite 500 ml", "Opcion ligera y fria.", 16m, "Bebidas", "bi bi-bottle-soda", "tone-success", "Lima limon", ["500 ml", "Fria"]),
                                        new PosProduct("agua-500", "Agua Mineral 500 ml", "Presentacion sencilla y fresca.", 9m, "Bebidas", "bi bi-droplet", "tone-primary", "Natural", ["500 ml", "Ligera"])
                                    ])
                            ]),
                        new PosCategoryNode(
                            "cervezas",
                            "Cervezas",
                            "Lager y premium para consumo en salon.",
                            "bi bi-cup-hot",
                            "tone-warning",
                            "Adultos",
                            Children:
                            [
                                new PosCategoryNode(
                                    "lager",
                                    "Lager",
                                    "Opciones ligeras para consumo frecuente.",
                                    "bi bi-cup",
                                    "tone-warning",
                                    "Clasicas",
                                    Products:
                                    [
                                        new PosProduct("pacena", "Pacena lata", "Cerveza helada de mostrador.", 17m, "Cervezas", "bi bi-cup", "tone-warning", "Lata", ["Lager", "Fria"]),
                                        new PosProduct("huari", "Huari lata", "Etiqueta premium para caja.", 18m, "Cervezas", "bi bi-cup", "tone-primary", "Premium", ["Lager", "Premium"])
                                    ]),
                                new PosCategoryNode(
                                    "premium",
                                    "Premium",
                                    "Mayor margen para clientes frecuentes.",
                                    "bi bi-stars",
                                    "tone-primary",
                                    "Especiales",
                                    Products:
                                    [
                                        new PosProduct("ipa-casa", "IPA de la casa", "Botella artesanal fria.", 24m, "Cervezas", "bi bi-stars", "tone-primary", "Artesanal", ["IPA", "Botella"]),
                                        new PosProduct("amber", "Amber Ale", "Perfil suave para grupos.", 23m, "Cervezas", "bi bi-stars", "tone-success", "Suave", ["Amber", "Botella"])
                                    ])
                            ]),
                        new PosCategoryNode(
                            "energizantes",
                            "Energizantes",
                            "Venta rapida para torneos y partidas largas.",
                            "bi bi-lightning-charge-fill",
                            "tone-danger",
                            "Alta energia",
                            Products:
                            [
                                new PosProduct("monster", "Monster 473 ml", "Lata fria de alta energia.", 22m, "Energizantes", "bi bi-lightning-charge-fill", "tone-danger", "473 ml", ["Lata", "Energia"]),
                                new PosProduct("speed", "Speed 473 ml", "Alternativa para consumo inmediato.", 21m, "Energizantes", "bi bi-lightning-fill", "tone-warning", "473 ml", ["Lata", "Rapido"])
                            ])
                    ]),
                new PosCategoryNode(
                    "snacks",
                    "Snacks",
                    "Papas, nachos y dulces para compra por impulso.",
                    "bi bi-bag-heart",
                    "tone-warning",
                    "Mostrador",
                    Children:
                    [
                        new PosCategoryNode(
                            "salados",
                            "Salados",
                            "Acompanamientos para bebidas y combos.",
                            "bi bi-egg-fried",
                            "tone-warning",
                            "Snack bar",
                            Children:
                            [
                                new PosCategoryNode(
                                    "papas",
                                    "Papas",
                                    "Presentaciones individuales y premium.",
                                    "bi bi-circle-square",
                                    "tone-warning",
                                    "Rotacion alta",
                                    Products:
                                    [
                                        new PosProduct("lays", "Papas Fritas", "Bolsa individual clasica.", 15m, "Snacks", "bi bi-circle-square", "tone-warning", "Clasico", ["Salado", "Bolsa"]),
                                        new PosProduct("nachos", "Nachos con Queso", "Bandeja para compartir.", 28m, "Snacks", "bi bi-triangle", "tone-danger", "Para compartir", ["Nachos", "Queso"])
                                    ]),
                                new PosCategoryNode(
                                    "mix",
                                    "Mix premium",
                                    "Snacks para ticket medio mas alto.",
                                    "bi bi-gem",
                                    "tone-primary",
                                    "Premium",
                                    Products:
                                    [
                                        new PosProduct("mani", "Mani salado", "Snack rapido para barra.", 10m, "Snacks", "bi bi-circle-fill", "tone-primary", "Barra", ["Mani", "Rapido"]),
                                        new PosProduct("mix-snack", "Mix crocante", "Combo pequeno para grupos.", 18m, "Snacks", "bi bi-gem", "tone-success", "Mix", ["Crocante", "Grupo"])
                                    ])
                            ]),
                        new PosCategoryNode(
                            "dulces",
                            "Dulces",
                            "Chocolates y galletas de venta casual.",
                            "bi bi-cookie",
                            "tone-success",
                            "Impulso",
                            Products:
                            [
                                new PosProduct("snickers", "Snickers", "Barra de chocolate individual.", 7m, "Dulces", "bi bi-cookie", "tone-success", "Barra", ["Chocolate", "Unidad"]),
                                new PosProduct("oreo", "Oreo mini", "Paquete pequeno de mostrador.", 6m, "Dulces", "bi bi-emoji-smile", "tone-primary", "Mini", ["Galleta", "Paquete"])
                            ])
                    ]),
                new PosCategoryNode(
                    "tiempo",
                    "Tiempo",
                    "Bloques de juego y extensiones para venta directa.",
                    "bi bi-stopwatch",
                    "tone-primary",
                    "Servicios",
                    Children:
                    [
                        new PosCategoryNode(
                            "juego-libre",
                            "Juego libre",
                            "Cobro por hora y medias horas.",
                            "bi bi-stopwatch-fill",
                            "tone-primary",
                            "Tiempo",
                            Products:
                            [
                                new PosProduct("hora-juego", "1 Hora de Juego", "Bloque base para una partida.", 40m, "Tiempo", "bi bi-stopwatch-fill", "tone-primary", "1 hora", ["Mesa", "Servicio"]),
                                new PosProduct("dos-horas", "2 Horas de Juego", "Paquete extendido para grupos.", 70m, "Tiempo", "bi bi-hourglass-split", "tone-warning", "2 horas", ["Mesa", "Servicio"])
                            ]),
                        new PosCategoryNode(
                            "adicionales",
                            "Adicionales",
                            "Extensiones rapidas para cerrar tickets.",
                            "bi bi-plus-circle",
                            "tone-success",
                            "Complementos",
                            Products:
                            [
                                new PosProduct("media-hora", "30 min extra", "Tiempo adicional de continuidad.", 18m, "Tiempo", "bi bi-plus-circle-fill", "tone-success", "Extra", ["30 min", "Servicio"]),
                                new PosProduct("taco-extra", "Prestamo de tacos", "Accesorio adicional de sala.", 6m, "Servicios", "bi bi-bezier2", "tone-primary", "Accesorio", ["Sala", "Soporte"])
                            ])
                    ]),
                new PosCategoryNode(
                    "combos",
                    "Combos",
                    "Packs rapidos para subir ticket promedio.",
                    "bi bi-box2-heart",
                    "tone-danger",
                    "Promociones",
                    Children:
                    [
                        new PosCategoryNode(
                            "duo",
                            "Para dos",
                            "Bebidas y snacks listos para parejas.",
                            "bi bi-people",
                            "tone-success",
                            "Duo",
                            Products:
                            [
                                new PosProduct("combo-duo", "Combo Amigos", "2 bebidas y 1 snack.", 45m, "Combos", "bi bi-people-fill", "tone-success", "Popular", ["2 bebidas", "1 snack"]),
                                new PosProduct("combo-duo-plus", "Combo Duo Plus", "2 cervezas y papas premium.", 52m, "Combos", "bi bi-stars", "tone-warning", "Mejor precio", ["2 cervezas", "Papas"])
                            ]),
                        new PosCategoryNode(
                            "grupal",
                            "Grupal",
                            "Packs para reservas o grupos casuales.",
                            "bi bi-people-fill",
                            "tone-primary",
                            "Grupo",
                            Products:
                            [
                                new PosProduct("combo-sala", "Combo Sala 4", "Bebida grande y 2 snacks.", 58m, "Combos", "bi bi-box-seam", "tone-primary", "4 personas", ["Grupo", "Compartir"]),
                                new PosProduct("combo-vip", "Combo Evento", "Pack premium para turno especial.", 92m, "Combos", "bi bi-gem", "tone-danger", "VIP", ["Premium", "Evento"])
                            ])
                    ])
            ],
            InitialOrderItems =
            [
                new OrderLineItem("hora-juego", "1 Hora de Juego", "Tiempo", 1, 40m, "bi bi-stopwatch-fill", "tone-primary"),
                new OrderLineItem("coca-500", "Coca Cola 500 ml", "Bebidas", 2, 16m, "bi bi-bottle-soda", "tone-danger"),
                new OrderLineItem("lays", "Papas Fritas", "Snacks", 1, 15m, "bi bi-circle-square", "tone-warning"),
                new OrderLineItem("nachos", "Nachos con Queso", "Snacks", 1, 28m, "bi bi-triangle", "tone-success")
            ]
        };
    }

    private void GoToRoot()
    {
        CurrentNode = null;
        SelectedPath.Clear();
    }

    private void GoBack()
    {
        if (SelectedPath.Count <= 1)
        {
            GoToRoot();
            return;
        }

        GoToPath(SelectedPath.Count - 2);
    }

    private void GoToPath(int index)
    {
        if (index < 0 || index >= SelectedPath.Count)
        {
            GoToRoot();
            return;
        }

        CurrentNode = SelectedPath[index];
        SelectedPath.RemoveRange(index + 1, SelectedPath.Count - (index + 1));
    }

    private void EnterCategory(PosCategoryNode category)
    {
        CurrentNode = category;

        if (SelectedPath.Count == 0)
        {
            SelectedPath.Add(category);
            return;
        }

        var lastNode = SelectedPath[^1];
        if (lastNode.Id == category.Id)
        {
            return;
        }

        SelectedPath.Add(category);
    }

    private void AddProduct(PosProduct product)
    {
        var existingItem = OrderItems.FirstOrDefault(item => item.ProductId == product.Id);
        if (existingItem is not null)
        {
            existingItem.Quantity += 1;
            return;
        }

        OrderItems.Add(new OrderLineItem(product.Id, product.Name, product.CategoryLabel, 1, product.Price, product.IconCss, product.ToneClass));
    }

    private void RemoveItem(string productId)
    {
        var item = OrderItems.FirstOrDefault(orderItem => orderItem.ProductId == productId);
        if (item is not null)
        {
            OrderItems.Remove(item);
        }
    }

    private void ChangeQuantity(string productId, int delta)
    {
        var item = OrderItems.FirstOrDefault(orderItem => orderItem.ProductId == productId);
        if (item is null)
        {
            return;
        }

        item.Quantity += delta;
        if (item.Quantity <= 0)
        {
            OrderItems.Remove(item);
        }
    }

    private string GetPanelTitle()
    {
        if (CurrentNode is null)
        {
            return "Categorias base";
        }

        return ShowingProducts
            ? $"Productos de {CurrentNode.Name}"
            : $"Subcategorias de {CurrentNode.Name}";
    }

    private string GetPanelDescription()
    {
        if (CurrentNode is null)
        {
            return "Primero se muestran las familias principales. Al entrar en una, aparecen sus categorias internas hasta llegar a productos.";
        }

        return ShowingProducts
            ? "Ya estas en una hoja final del arbol. Desde aqui se agregan productos al detalle de la venta."
            : "Esta categoria todavia contiene otras categorias. Sigue navegando hasta llegar a su lista final de productos.";
    }

    private string GetStageHeading()
    {
        if (CurrentNode is null)
        {
            return "Selecciona una categoria principal";
        }

        return ShowingProducts
            ? "Productos listos para vender"
            : "Todavia estas navegando por categorias";
    }

    private string GetStageBannerText()
    {
        if (CurrentNode is null)
        {
            return "Haz clic en una categoria base para abrir sus subcategorias.";
        }

        return ShowingProducts
            ? "Haz clic en agregar para mandar el producto al costado derecho."
            : "La categoria actual todavia se compone de otras categorias hijas.";
    }

    private string GetStageCountLabel()
    {
        return ShowingProducts
            ? $"{VisibleProducts.Count} productos"
            : $"{VisibleCategories.Count} categorias";
    }

    private string GetExplorerLabel()
    {
        if (CurrentNode is null)
        {
            return $"{VisibleCategories.Count} categorias base";
        }

        return ShowingProducts
            ? $"{VisibleProducts.Count} productos disponibles"
            : $"{VisibleCategories.Count} subcategorias";
    }

    private static string GetNodeCountLabel(PosCategoryNode category)
    {
        return category.Children.Count > 0
            ? $"{category.Children.Count} subcategorias"
            : $"{category.Products.Count} productos";
    }

    private static string GetNodeActionLabel(PosCategoryNode category)
    {
        return category.Children.Count > 0
            ? "Entrar a la siguiente capa"
            : "Abrir lista de productos";
    }

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }

    private sealed class VentaPreviewViewModel
    {
        public string BrandSubtitle { get; init; } = string.Empty;
        public string ShiftLabel { get; init; } = string.Empty;
        public string ShiftStatus { get; init; } = string.Empty;
        public string ShiftTimeLabel { get; init; } = string.Empty;
        public string CashierName { get; init; } = string.Empty;
        public string CashierRole { get; init; } = string.Empty;
        public string SearchPlaceholder { get; init; } = string.Empty;
        public string CustomerSearchPlaceholder { get; init; } = string.Empty;
        public decimal DiscountAmount { get; init; }
        public decimal ServiceCharge { get; init; }
        public IReadOnlyList<PaymentMethod> PaymentMethods { get; init; } = [];
        public IReadOnlyList<PosCategoryNode> RootCategories { get; init; } = [];
        public IReadOnlyList<OrderLineItem> InitialOrderItems { get; init; } = [];
    }

    private sealed record PaymentMethod(string Name, string IconCss, bool IsPrimary);

    private sealed class PosCategoryNode(
        string id,
        string name,
        string description,
        string iconCss,
        string toneClass,
        string cardCaption,
        IReadOnlyList<PosCategoryNode>? Children = null,
        IReadOnlyList<PosProduct>? Products = null)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public string IconCss { get; } = iconCss;
        public string ToneClass { get; } = toneClass;
        public string CardCaption { get; } = cardCaption;
        public IReadOnlyList<PosCategoryNode> Children { get; } = Children ?? [];
        public IReadOnlyList<PosProduct> Products { get; } = Products ?? [];
    }

    private sealed record PosProduct(
        string Id,
        string Name,
        string Description,
        decimal Price,
        string CategoryLabel,
        string IconCss,
        string ToneClass,
        string MediaLabel,
        IReadOnlyList<string> Tags);

    private sealed class OrderLineItem(
        string productId,
        string name,
        string detail,
        int quantity,
        decimal unitPrice,
        string iconCss,
        string toneClass)
    {
        public string ProductId { get; } = productId;
        public string Name { get; } = name;
        public string Detail { get; } = detail;
        public int Quantity { get; set; } = quantity;
        public decimal UnitPrice { get; } = unitPrice;
        public string IconCss { get; } = iconCss;
        public string ToneClass { get; } = toneClass;
        public decimal Total => Quantity * UnitPrice;
    }
}
