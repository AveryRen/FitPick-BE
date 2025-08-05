    using System;
using System.Collections.Generic;
using FitPick.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace FitPick.Repository.Context;

public partial class FitPickContext : DbContext
{
    public FitPickContext()
    {
    }

    public FitPickContext(DbContextOptions<FitPickContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blogpost> Blogposts { get; set; }

    public virtual DbSet<Chatbotlog> Chatbotlogs { get; set; }

    public virtual DbSet<Favoritemeal> Favoritemeals { get; set; }

    public virtual DbSet<Healthprofile> Healthprofiles { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Locationingredient> Locationingredients { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<Mealingredient> Mealingredients { get; set; }

    public virtual DbSet<Mealplan> Mealplans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Shoppinglist> Shoppinglists { get; set; }

    public virtual DbSet<Spendinglog> Spendinglogs { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=db.myzpdmmkqowmaetmlejy.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=!MrFCq9d?7cGR7v;Ssl Mode=Require;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("blog_status_enum", new[] { "Published", "Draft", "Archived" })
            .HasPostgresEnum("gender_enum", new[] { "Male", "Female" })
            .HasPostgresEnum("location_type_enum", new[] { "Store", "Market", "Restaurant" })
            .HasPostgresEnum("meal_category_enum", new[] { "Breakfast", "Lunch", "Dinner", "Snack" })
            .HasPostgresEnum("meal_status_enum", new[] { "Published", "Draft" })
            .HasPostgresEnum("meal_time_enum", new[] { "Breakfast", "Lunch", "Dinner", "Snack" })
            .HasPostgresEnum("notification_type_enum", new[] { "Reminder", "Promotion", "Alert" })
            .HasPostgresEnum("plan_status_enum", new[] { "Active", "Swapped", "Deleted" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("shopping_status_enum", new[] { "Pending", "Purchased", "Unavailable" })
            .HasPostgresEnum("subscription_plan_enum", new[] { "Monthly", "Yearly" })
            .HasPostgresEnum("subscription_status_enum", new[] { "Active", "Expired", "Canceled" })
            .HasPostgresEnum("user_role_enum", new[] { "Guest", "User", "Premium", "Admin" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Blogpost>(entity =>
        {
            entity.HasKey(e => e.Postid).HasName("blogposts_pkey");

            entity.ToTable("blogposts");

            entity.Property(e => e.Postid).HasColumnName("postid");
            entity.Property(e => e.Authorid).HasColumnName("authorid");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Shortdescription).HasColumnName("shortdescription");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogposts)
                .HasForeignKey(d => d.Authorid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("blogposts_authorid_fkey");
        });

        modelBuilder.Entity<Chatbotlog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("chatbotlogs_pkey");

            entity.ToTable("chatbotlogs");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Question).HasColumnName("question");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Chatbotlogs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("chatbotlogs_userid_fkey");
        });

        modelBuilder.Entity<Favoritemeal>(entity =>
        {
            entity.HasKey(e => e.Favoriteid).HasName("favoritemeals_pkey");

            entity.ToTable("favoritemeals");

            entity.Property(e => e.Favoriteid).HasColumnName("favoriteid");
            entity.Property(e => e.Mealid).HasColumnName("mealid");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Meal).WithMany(p => p.Favoritemeals)
                .HasForeignKey(d => d.Mealid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("favoritemeals_mealid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Favoritemeals)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("favoritemeals_userid_fkey");
        });

        modelBuilder.Entity<Healthprofile>(entity =>
        {
            entity.HasKey(e => e.Profileid).HasName("healthprofiles_pkey");

            entity.ToTable("healthprofiles");

            entity.Property(e => e.Profileid).HasColumnName("profileid");
            entity.Property(e => e.Allergies).HasColumnName("allergies");
            entity.Property(e => e.Chronicdiseases).HasColumnName("chronicdiseases");
            entity.Property(e => e.Dailymeals).HasColumnName("dailymeals");
            entity.Property(e => e.Dietarypreferences).HasColumnName("dietarypreferences");
            entity.Property(e => e.Healthgoal)
                .HasMaxLength(50)
                .HasColumnName("healthgoal");
            entity.Property(e => e.Lifestyle)
                .HasMaxLength(50)
                .HasColumnName("lifestyle");
            entity.Property(e => e.Religiondiet)
                .HasMaxLength(100)
                .HasColumnName("religiondiet");
            entity.Property(e => e.Targetcalories).HasColumnName("targetcalories");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Healthprofiles)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("healthprofiles_userid_fkey");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Ingredientid).HasName("ingredients_pkey");

            entity.ToTable("ingredients");

            entity.Property(e => e.Ingredientid).HasColumnName("ingredientid");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Locationid).HasName("locations_pkey");

            entity.ToTable("locations");

            entity.Property(e => e.Locationid).HasColumnName("locationid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 6)
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Locationingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("locationingredients_pkey");

            entity.ToTable("locationingredients");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Availability)
                .HasDefaultValue(true)
                .HasColumnName("availability");
            entity.Property(e => e.Ingredientid).HasColumnName("ingredientid");
            entity.Property(e => e.Locationid).HasColumnName("locationid");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Locationingredients)
                .HasForeignKey(d => d.Ingredientid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("locationingredients_ingredientid_fkey");

            entity.HasOne(d => d.Location).WithMany(p => p.Locationingredients)
                .HasForeignKey(d => d.Locationid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("locationingredients_locationid_fkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Mealid).HasName("meals_pkey");

            entity.ToTable("meals");

            entity.Property(e => e.Mealid).HasColumnName("mealid");
            entity.Property(e => e.Calories).HasColumnName("calories");
            entity.Property(e => e.Cookingtime).HasColumnName("cookingtime");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Diettype)
                .HasMaxLength(50)
                .HasColumnName("diettype");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Meals)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("meals_createdby_fkey");
        });

        modelBuilder.Entity<Mealingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mealingredients_pkey");

            entity.ToTable("mealingredients");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ingredientid).HasColumnName("ingredientid");
            entity.Property(e => e.Mealid).HasColumnName("mealid");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Mealingredients)
                .HasForeignKey(d => d.Ingredientid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealingredients_ingredientid_fkey");

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealingredients)
                .HasForeignKey(d => d.Mealid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealingredients_mealid_fkey");
        });

        modelBuilder.Entity<Mealplan>(entity =>
        {
            entity.HasKey(e => e.Planid).HasName("mealplans_pkey");

            entity.ToTable("mealplans");

            entity.Property(e => e.Planid).HasColumnName("planid");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Mealid).HasColumnName("mealid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealplans)
                .HasForeignKey(d => d.Mealid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealplans_mealid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Mealplans)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealplans_userid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.Notificationid).HasColumnName("notificationid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_userid_fkey");
        });

        modelBuilder.Entity<Shoppinglist>(entity =>
        {
            entity.HasKey(e => e.Listid).HasName("shoppinglists_pkey");

            entity.ToTable("shoppinglists");

            entity.Property(e => e.Listid).HasColumnName("listid");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Itemname)
                .HasMaxLength(100)
                .HasColumnName("itemname");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Shoppinglists)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shoppinglists_userid_fkey");
        });

        modelBuilder.Entity<Spendinglog>(entity =>
        {
            entity.HasKey(e => e.Spendingid).HasName("spendinglogs_pkey");

            entity.ToTable("spendinglogs");

            entity.Property(e => e.Spendingid).HasColumnName("spendingid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Spendinglogs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("spendinglogs_userid_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Subscriptionid).HasName("subscriptions_pkey");

            entity.ToTable("subscriptions");

            entity.Property(e => e.Subscriptionid).HasColumnName("subscriptionid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Enddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("enddate");
            entity.Property(e => e.Startdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("startdate");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Height)
                .HasPrecision(5, 2)
                .HasColumnName("height");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Weight)
                .HasPrecision(5, 2)
                .HasColumnName("weight");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
