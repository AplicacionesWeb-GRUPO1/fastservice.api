﻿using FastService.API.FastService.Domain.Models;
using FastService.API.FastService.Domain.Repositories;
using FastService.API.FastService.Domain.Services;
using FastService.API.FastService.Domain.Services.Communication;

namespace FastService.API.FastService.Services;

public class GalleryService : IGalleryService
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IExpertRepository _expertRepository;

    private readonly IUnitOfWork _unitOfWork;

    public GalleryService(IGalleryRepository GalleryRepository, IUnitOfWork unitOfWork, IClientRepository clientRepository)
    {
        _galleryRepository = GalleryRepository;
        _unitOfWork = unitOfWork;
        _expertRepository = clientRepository;
    }

    public async Task<IEnumerable<Gallery>> ListAsync()
    {
        return await _galleryRepository.ListAsync();
    }

    public async Task<IEnumerable<Gallery>> ListByExpertIdAsync(int clientId)
    {
        return await _galleryRepository.FindByExpertIdAsync(clientId);
    }

    public async Task<GalleryResponse> SaveAsync(Gallery Gallery)
    {
        // Validate Expert Id

        var existingExpert = await _expertRepository.FindByIdAsync(Gallery.ExpertId);

        if (existingExpert == null)
            return new GalleryResponse("Invalid Expert");
        
        
        try
        {
            // Add Gallery
            await _galleryRepository.AddAsync(Gallery);
            
            // Complete Transaction
            await _unitOfWork.CompleteAsync();
            
            // Return response
            return new GalleryResponse(Gallery);

        }
        catch (Exception e)
        {
            // Error Handling
            return new GalleryResponse($"An error occurred while saving the Gallery: {e.Message}");
        }

        
    }

    public async Task<GalleryResponse> UpdateAsync(int GalleryId, Gallery Gallery)  
    {
        var existingGallery = await _galleryRepository.FindByIdAsync(GalleryId);
        
        // Validate Gallery

        if (existingGallery == null)
            return new GalleryResponse("Gallery not found.");

        // Validate ClientId

        var existingClient = await _expertRepository.FindByIdAsync(Gallery.ClientId);

        if (existingClient == null)
            return new GalleryResponse("Invalid Client");
        
        // Validate Title

        var existingGalleryWithTitle = await _galleryRepository.FindByTitleAsync(Gallery.Title);

        if (existingGalleryWithTitle != null && existingGalleryWithTitle.Id != existingGallery.Id)
            return new GalleryResponse("Gallery title already exists.");

        // Modify Fields
        existingGallery.Address = Gallery.Address;
        existingGallery.Title = Gallery.Title;
        existingGallery.Description = Gallery.Description;
        existingGallery.IsPublished = Gallery.IsPublished;

        try
        {
            _galleryRepository.Update(existingGallery);
            await _unitOfWork.CompleteAsync();

            return new GalleryResponse(existingGallery);
            
        }
        catch (Exception e)
        {
            // Error Handling
            return new GalleryResponse($"An error occurred while updating the Gallery: {e.Message}");
        }

    }

    public async Task<GalleryResponse> DeleteAsync(int GalleryId)
    {
        var existingGallery = await _galleryRepository.FindByIdAsync(GalleryId);
        
        // Validate Gallery

        if (existingGallery == null)
            return new GalleryResponse("Gallery not found.");
        
        try
        {
            _galleryRepository.Remove(existingGallery);
            await _unitOfWork.CompleteAsync();

            return new GalleryResponse(existingGallery);
            
        }
        catch (Exception e)
        {
            // Error Handling
            return new GalleryResponse($"An error occurred while deleting the Gallery: {e.Message}");
        }

    }
}