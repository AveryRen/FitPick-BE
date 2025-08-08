using System;
using System.Collections.Generic;
using FitPick_EXE201.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models;

public partial class FitPickDbContext : DbContext
{
    public FitPickDbContext()
    {
    }

    public FitPickDbContext(DbContextOptions<FitPickDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogStatus> BlogStatuses { get; set; }

    public virtual DbSet<Blogpost> Blogposts { get; set; }

    public virtual DbSet<Chatbotlog> Chatbotlogs { get; set; }

    public virtual DbSet<Favoritemeal> Favoritemeals { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Healthprofile> Healthprofiles { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<LocationType> LocationTypes { get; set; }

    public virtual DbSet<Locationingredient> Locationingredients { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<MealCategory> MealCategories { get; set; }

    public virtual DbSet<MealStatus> MealStatuses { get; set; }

    public virtual DbSet<MealTime> MealTimes { get; set; }

    public virtual DbSet<Mealingredient> Mealingredients { get; set; }

    public virtual DbSet<Mealplan> Mealplans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<PlanStatus> PlanStatuses { get; set; }

    public virtual DbSet<ShoppingStatus> ShoppingStatuses { get; set; }

    public virtual DbSet<Shoppinglist> Shoppinglists { get; set; }

    public virtual DbSet<Spendinglog> Spendinglogs { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.myzpdmmkqowmaetmlejy;Password=!MrFCq9d?7cGR7v;SSL Mode=Require;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<BlogStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("blog_statuses_pkey");
        });

        modelBuilder.Entity<Blogpost>(entity =>
        {
            entity.HasKey(e => e.Postid).HasName("blogposts_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.StatusId).HasDefaultValue(2);
            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogposts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("blogposts_authorid_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Blogposts).HasConstraintName("blogposts_status_id_fkey");
        });

        modelBuilder.Entity<Chatbotlog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("chatbotlogs_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.User).WithMany(p => p.Chatbotlogs)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("chatbotlogs_userid_fkey");
        });

        modelBuilder.Entity<Favoritemeal>(entity =>
        {
            entity.HasKey(e => e.Favoriteid).HasName("favoritemeals_pkey");

            entity.HasOne(d => d.Meal).WithMany(p => p.Favoritemeals)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("favoritemeals_mealid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Favoritemeals)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("favoritemeals_userid_fkey");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genders_pkey");
        });

        modelBuilder.Entity<Healthprofile>(entity =>
        {
            entity.HasKey(e => e.Profileid).HasName("healthprofiles_pkey");

            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.User).WithMany(p => p.Healthprofiles)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("healthprofiles_userid_fkey");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Ingredientid).HasName("ingredients_pkey");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Locationid).HasName("locations_pkey");

            entity.HasOne(d => d.Type).WithMany(p => p.Locations).HasConstraintName("locations_type_id_fkey");
        });

        modelBuilder.Entity<LocationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("location_types_pkey");
        });

        modelBuilder.Entity<Locationingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("locationingredients_pkey");

            entity.Property(e => e.Availability).HasDefaultValue(true);

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Locationingredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("locationingredients_ingredientid_fkey");

            entity.HasOne(d => d.Location).WithMany(p => p.Locationingredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("locationingredients_locationid_fkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Mealid).HasName("meals_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.StatusId).HasDefaultValue(2);

            entity.HasOne(d => d.Category).WithMany(p => p.Meals).HasConstraintName("meals_category_id_fkey");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Meals)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("meals_createdby_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Meals).HasConstraintName("meals_status_id_fkey");
        });

        modelBuilder.Entity<MealCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meal_categories_pkey");
        });

        modelBuilder.Entity<MealStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meal_statuses_pkey");
        });

        modelBuilder.Entity<MealTime>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meal_times_pkey");
        });

        modelBuilder.Entity<Mealingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mealingredients_pkey");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Mealingredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealingredients_ingredientid_fkey");

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealingredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealingredients_mealid_fkey");
        });

        modelBuilder.Entity<Mealplan>(entity =>
        {
            entity.HasKey(e => e.Planid).HasName("mealplans_pkey");

            entity.Property(e => e.StatusId).HasDefaultValue(1);

            entity.HasOne(d => d.Meal).WithMany(p => p.Mealplans)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealplans_mealid_fkey");

            entity.HasOne(d => d.Mealtime).WithMany(p => p.Mealplans).HasConstraintName("mealplans_mealtime_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Mealplans).HasConstraintName("mealplans_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Mealplans)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("mealplans_userid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notifications_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.Isread).HasDefaultValue(false);

            entity.HasOne(d => d.Type).WithMany(p => p.Notifications).HasConstraintName("notifications_type_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_userid_fkey");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_types_pkey");
        });

        modelBuilder.Entity<PlanStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plan_statuses_pkey");
        });

        modelBuilder.Entity<ShoppingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shopping_statuses_pkey");
        });

        modelBuilder.Entity<Shoppinglist>(entity =>
        {
            entity.HasKey(e => e.Listid).HasName("shoppinglists_pkey");

            entity.Property(e => e.StatusId).HasDefaultValue(1);

            entity.HasOne(d => d.Status).WithMany(p => p.Shoppinglists).HasConstraintName("shoppinglists_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Shoppinglists)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shoppinglists_userid_fkey");
        });

        modelBuilder.Entity<Spendinglog>(entity =>
        {
            entity.HasKey(e => e.Spendingid).HasName("spendinglogs_pkey");

            entity.HasOne(d => d.User).WithMany(p => p.Spendinglogs)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("spendinglogs_userid_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Subscriptionid).HasName("subscriptions_pkey");

            entity.Property(e => e.StatusId).HasDefaultValue(1);

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions).HasConstraintName("subscriptions_plan_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Subscriptions).HasConstraintName("subscriptions_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_userid_fkey");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_plans_pkey");
        });

        modelBuilder.Entity<SubscriptionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_statuses_pkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.GenderId).HasColumnName("gender_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Createdat)
                .HasColumnName("createdat")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");
            entity.Property(e => e.Updatedat)
                .HasColumnName("updatedat")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.City).HasColumnName("city");

             entity.HasOne(d => d.Gender)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.GenderId)
                .HasConstraintName("users_gender_id_fkey");

             entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("users_role_id_fkey");
        });



        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_roles_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
