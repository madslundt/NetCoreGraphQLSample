using Microsoft.EntityFrameworkCore;
namespace DataModel.Models
{
    public class CommentContext
    {
        public static void Build(ModelBuilder builder)
        {
            builder.Entity<Comment>(b =>
            {
                b.Property(p => p.Id)
                    .IsRequired();

                b.Property(p => p.Text)
                    .IsRequired();

                b.Property(p => p.Created)
                    .IsRequired();

                b.HasOne(r => r.User)
                    .WithMany(r => r.Comments)
                    .HasForeignKey(fk => fk.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasKey(k => k.Id);
            });
        }
    }
}
