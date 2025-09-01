using System;
using System.Collections.Generic;
using FitPick_EXE201.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Data;

public partial class FitPickContext : DbContext
{
    public FitPickContext()
    {
    }

    public FitPickContext(DbContextOptions<FitPickContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogMedium> BlogMedia { get; set; }

    public virtual DbSet<Blogpost> Blogposts { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Healthgoal> Healthgoals { get; set; }

    public virtual DbSet<Healthprofile> Healthprofiles { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Lifestyle> Lifestyles { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<MealCategory> MealCategories { get; set; }

    public virtual DbSet<MealReview> MealReviews { get; set; }

    public virtual DbSet<MealStatus> MealStatuses { get; set; }

    public virtual DbSet<MealTime> MealTimes { get; set; }

    public virtual DbSet<Mealingredient> Mealingredients { get; set; }

    public virtual DbSet<Mealplan> Mealplans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<PlanStatus> PlanStatuses { get; set; }

    public virtual DbSet<Spendinglog> Spendinglogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserIngredient> UserIngredients { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.myzpdmmkqowmaetmlejy;Password=!MrFCq9d?7cGR7v;SSL Mode=Require;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("blog_category_pkey");
        });

        modelBuilder.Entity<BlogMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("blog_media_pkey");

            entity.Property(e => e.OrderIndex).HasDefaultValue(0);

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogMedia).HasConstraintName("blog_media_blog_id_fkey");
        });

        modelBuilder.Entity<Blogpost>(entity =>
        {
            entity.HasKey(e => e.Postid).HasName("blogposts_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValue(false);
            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogposts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("blogposts_authorid_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Blogposts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("blogposts_categoryid_fkey");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genders_pkey");
        });

        modelBuilder.Entity<Healthgoal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("healthgoals_pkey");
        });

        modelBuilder.Entity<Healthprofile>(entity =>
        {
            entity.HasKey(e => e.Profileid).HasName("healthprofiles_pkey");

            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Healthgoal).WithMany(p => p.Healthprofiles).HasConstraintName("healthprofiles_healthgoalid_fkey");

            entity.HasOne(d => d.Lifestyle).WithMany(p => p.Healthprofiles).HasConstraintName("healthprofiles_lifestyleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Healthprofiles)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("healthprofiles_userid_fkey");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Ingredientid).HasName("ingredients_pkey");

            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<Lifestyle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lifestyles_pkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Mealid).HasName("meals_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.IsPremium).HasDefaultValue(false);
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

        modelBuilder.Entity<MealReview>(entity =>
        {
            entity.HasKey(e => e.Reviewid).HasName("meal_reviews_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.IsFavorite).HasDefaultValue(false);
            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Meal).WithMany(p => p.MealReviews).HasConstraintName("meal_reviews_mealid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.MealReviews).HasConstraintName("meal_reviews_userid_fkey");
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

        modelBuilder.Entity<Spendinglog>(entity =>
        {
            entity.HasKey(e => e.Spendingid).HasName("spendinglogs_pkey");

            entity.HasOne(d => d.User).WithMany(p => p.Spendinglogs)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("spendinglogs_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
            entity.Property(e => e.RoleId).HasDefaultValue(2);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Gender).WithMany(p => p.Users).HasConstraintName("users_gender_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<UserIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_ingredients_pkey");

            entity.Property(e => e.Updatedat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.UserIngredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_ingredients_ingredientid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserIngredients)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_ingredients_userid_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_roles_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
