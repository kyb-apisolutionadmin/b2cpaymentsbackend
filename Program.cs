
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//Stripe services here



            builder.Services.AddControllers();
            builder.Services.AddSingleton<Code4GoodBackEnd.Services.IPaymentsGateway>(x => {
                var logger = x.GetRequiredService<ILogger<Code4GoodBackEnd.Services.StripePaymentsGateway>>();
                string stripeSecretKey = builder.Configuration.GetSection("Stripe").GetValue<string>("secretKey");
                string stripePublicKey = builder.Configuration.GetSection("Stripe").GetValue<string>("publicKey");

                if (string.IsNullOrEmpty(stripeSecretKey) || string.IsNullOrEmpty(stripePublicKey))
                    logger.LogError("It looks like the Stripe keys are missing.");
                return new Code4GoodBackEnd.Services.StripePaymentsGateway(logger, stripeSecretKey);
            });
       



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

