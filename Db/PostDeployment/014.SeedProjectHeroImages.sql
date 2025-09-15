-- =============================================
-- Seed Data for Project Hero Images
-- Created: 2025-01-14
-- Description: Adds hero image blob references to demo projects
-- Uses structured blob paths following the FileStorageService pattern
-- =============================================

PRINT '[014.SeedProjectHeroImages.sql] Starting hero image seed for demo projects...';

-- Update projects with structured blob paths
-- Format: public-assets/projects/{project-key}/hero-image.jpg
-- These would be actual blobs in Azure Storage in production

-- Update Tech projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] IN ('TECH-ESCAZU-001', 'INNOV-DEMO')
  AND [HeroImageBlobId] IS NULL;

-- Update Bio/Health projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] IN ('BIO-SANTAANA-002', 'HEALTH-TRESRIOS-008')
  AND [HeroImageBlobId] IS NULL;

-- Update Education projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'EDU-HEREDIA-003'
  AND [HeroImageBlobId] IS NULL;

-- Update Agriculture projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'AGRO-CARTAGO-004'
  AND [HeroImageBlobId] IS NULL;

-- Update Logistics projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'LOG-ALAJUELA-005'
  AND [HeroImageBlobId] IS NULL;

-- Update Finance projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'FIN-MORAVIA-006'
  AND [HeroImageBlobId] IS NULL;

-- Update Green/Environmental projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'GREEN-CURRI-007'
  AND [HeroImageBlobId] IS NULL;

-- Update Tourism projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'TOURISM-BELEN-009'
  AND [HeroImageBlobId] IS NULL;

-- Update Social projects
UPDATE [businessincubators].[Projects]
SET [HeroImageBlobId] = 'public-assets/projects/' + LOWER([Key]) + '/hero-image.jpg',
    [HasHeroImage] = 1
WHERE [Key] = 'SOCIAL-DESAMP-010'
  AND [HeroImageBlobId] IS NULL;

-- Note: These blob paths follow the IFileStorageService structure:
-- Container: public-assets (for publicly accessible project images)
-- Path: projects/{project-key}/hero-image.jpg
--
-- In development, you can either:
-- 1. Upload actual images to Azure Blob Storage using the container "public-assets"
-- 2. Use local storage emulator (Azurite) with sample images
-- 3. Add fallback images in wwwroot/images/projects/ for development

PRINT '[014.SeedProjectHeroImages.sql] Completed adding hero image blob references to demo projects';
PRINT '[014.SeedProjectHeroImages.sql] Note: Upload actual images to Azure Blob Storage container "public-assets"';
PRINT '[014.SeedProjectHeroImages.sql] Path format: public-assets/projects/{project-key}/hero-image.jpg';