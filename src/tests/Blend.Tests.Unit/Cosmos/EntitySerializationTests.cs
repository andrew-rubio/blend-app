using System.Text.Json;
using Blend.Domain.Entities;
using Xunit;

namespace Blend.Tests.Unit.Cosmos;

/// <summary>
/// Unit tests that verify entity types round-trip through JSON serialisation correctly.
/// </summary>
public class EntitySerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    [Fact]
    public void User_RoundTripsJson()
    {
        var user = new User
        {
            Id = "u1",
            Email = "test@example.com",
            DisplayName = "Test User",
            ProfilePhotoUrl = "https://example.com/photo.jpg",
            Role = UserRole.Admin,
            MeasurementUnit = MeasurementUnit.Imperial,
            UnreadNotificationCount = 3,
            Preferences = new UserPreferences
            {
                FavoriteCuisines = ["Italian"],
                Diets = ["vegan"],
                Intolerances = ["gluten"],
                DislikedIngredientIds = ["ing-1"],
            },
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
        };

        var json = JsonSerializer.Serialize(user, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<User>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(user.Id, deserialized.Id);
        Assert.Equal(user.Email, deserialized.Email);
        Assert.Equal(user.DisplayName, deserialized.DisplayName);
        Assert.Equal(user.ProfilePhotoUrl, deserialized.ProfilePhotoUrl);
        Assert.Equal(user.Role, deserialized.Role);
        Assert.Equal(user.MeasurementUnit, deserialized.MeasurementUnit);
        Assert.Equal(user.UnreadNotificationCount, deserialized.UnreadNotificationCount);
        Assert.Equal(user.Preferences.FavoriteCuisines, deserialized.Preferences.FavoriteCuisines);
        Assert.Equal(user.Preferences.Diets, deserialized.Preferences.Diets);
        Assert.Equal(user.CreatedAt, deserialized.CreatedAt);
        Assert.Equal(user.UpdatedAt, deserialized.UpdatedAt);
    }

    [Fact]
    public void Recipe_RoundTripsJson()
    {
        var recipe = new Recipe
        {
            Id = "r1",
            AuthorId = "u1",
            Title = "Pasta",
            Description = "Yummy pasta",
            Ingredients =
            [
                new RecipeIngredient { Quantity = 200, Unit = "g", IngredientName = "spaghetti", IngredientId = "ing-1" },
            ],
            Directions =
            [
                new RecipeDirection { StepNumber = 1, Text = "Boil water.", MediaUrl = null },
            ],
            PrepTime = 10,
            CookTime = 20,
            Servings = 2,
            CuisineType = "Italian",
            DishType = "main course",
            Tags = ["quick", "easy"],
            IsPublic = true,
            LikeCount = 5,
            ViewCount = 100,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(recipe, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Recipe>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(recipe.Id, deserialized.Id);
        Assert.Equal(recipe.AuthorId, deserialized.AuthorId);
        Assert.Equal(recipe.Title, deserialized.Title);
        Assert.Single(deserialized.Ingredients);
        Assert.Equal(200, deserialized.Ingredients[0].Quantity);
        Assert.Single(deserialized.Directions);
        Assert.Equal(1, deserialized.Directions[0].StepNumber);
        Assert.Equal(recipe.Tags, deserialized.Tags);
        Assert.Equal(recipe.IsPublic, deserialized.IsPublic);
    }

    [Fact]
    public void Connection_RoundTripsJson()
    {
        var connection = new Connection
        {
            Id = "c1",
            UserId = "u1",
            FriendUserId = "u2",
            Status = ConnectionStatus.Accepted,
            InitiatedBy = "u1",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(connection, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Connection>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(connection.Id, deserialized.Id);
        Assert.Equal(ConnectionStatus.Accepted, deserialized.Status);
        Assert.Equal(connection.InitiatedBy, deserialized.InitiatedBy);
    }

    [Fact]
    public void Activity_RoundTripsJson()
    {
        var activity = new Activity
        {
            Id = "a1",
            UserId = "u1",
            Type = ActivityType.Cooked,
            ReferenceId = "r1",
            ReferenceType = "recipe",
            Timestamp = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(activity, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Activity>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(ActivityType.Cooked, deserialized.Type);
        Assert.Equal(activity.ReferenceId, deserialized.ReferenceId);
    }

    [Fact]
    public void CookingSession_RoundTripsJson()
    {
        var session = new CookingSession
        {
            Id = "cs1",
            UserId = "u1",
            Dishes =
            [
                new CookingSessionDish
                {
                    DishId = "d1",
                    Name = "Pasta",
                    CuisineType = "Italian",
                    Notes = "Al dente",
                    Ingredients =
                    [
                        new SessionIngredient { IngredientId = "ing-spaghetti", Name = "spaghetti", AddedAt = DateTimeOffset.UtcNow },
                    ],
                },
            ],
            Status = CookingSessionStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(session, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<CookingSession>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(CookingSessionStatus.Active, deserialized.Status);
        Assert.Single(deserialized.Dishes);
        Assert.Equal("Pasta", deserialized.Dishes[0].Name);
    }

    [Fact]
    public void Notification_RoundTripsJson()
    {
        var notification = new Notification
        {
            Id = "n1",
            RecipientUserId = "u1",
            Type = NotificationType.FriendRequestReceived,
            SourceUserId = "u2",
            Message = "You have a friend request",
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 7776000,
        };

        var json = JsonSerializer.Serialize(notification, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Notification>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(NotificationType.FriendRequestReceived, deserialized.Type);
        Assert.Equal(7776000, deserialized.Ttl);
        Assert.False(deserialized.Read);
    }

    [Fact]
    public void Content_RoundTripsJson()
    {
        var content = new Content
        {
            Id = "ct1",
            ContentType = ContentType.Story,
            Title = "Welcome",
            Body = "Hello world",
            IsPublished = true,
            PublishedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(content, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Content>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(ContentType.Story, deserialized.ContentType);
        Assert.True(deserialized.IsPublished);
    }

    [Fact]
    public void CacheEntry_RoundTripsJson()
    {
        var entry = new CacheEntry
        {
            CacheKey = "spoon:search:abc123",
            Data = """{"results":[]}""",
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 3600,
        };

        var json = JsonSerializer.Serialize(entry, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<CacheEntry>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(entry.CacheKey, deserialized.CacheKey);
        Assert.Equal(entry.Data, deserialized.Data);
        Assert.Equal(3600, deserialized.Ttl);
    }

    [Fact]
    public void CacheEntry_IdMatchesCacheKey()
    {
        var entry = new CacheEntry { CacheKey = "spoon:recipe:42" };
        Assert.Equal("spoon:recipe:42", entry.Id);
    }

    [Fact]
    public void IngredientPairing_RoundTripsJson()
    {
        var pairing = new IngredientPairing
        {
            Id = "ip1",
            IngredientId = "garlic",
            PairedIngredientId = "olive-oil",
            Score = 0.95,
            CoOccurrenceCount = 1500,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(pairing, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<IngredientPairing>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(0.95, deserialized.Score);
        Assert.Equal(1500, deserialized.CoOccurrenceCount);
    }
}
