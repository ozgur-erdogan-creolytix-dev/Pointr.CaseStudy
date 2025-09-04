using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pointr.CaseStudy.Domain.Entities;
using Pointr.CaseStudy.Infrastructure.Persistence.Database.Configurations;

namespace Pointr.CaseStudy.Infrastructure.Persistence.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Page> Pages
    {
        get { return Set<Page>(); }
    }

    public DbSet<PageDraft> PageDrafts
    {
        get { return Set<PageDraft>(); }
    }

    public DbSet<PagePublication> PagePublications
    {
        get { return Set<PagePublication>(); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PageConfiguration());
        modelBuilder.ApplyConfiguration(new PageDraftConfiguration());
        modelBuilder.ApplyConfiguration(new PagePublicationConfiguration());
    }
}
