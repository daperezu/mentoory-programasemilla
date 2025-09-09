# REQ-006: Bidirectional Feedback Conversation System (Simplified)

## Status: ✅ COMPLETED
## Created: 2025-09-09
## Completed: 2025-09-09
## Priority: High
## Actual Effort: ~5 hours

### Completion Notes
- **Architecture Pivot**: Implemented using partial views instead of API for simplicity
- **Progressive Enhancement**: Works without JavaScript, enhanced with AJAX
- **Full Feature Set**: All requirements met including conversations, status management, and navigation
- **Clean Build**: 0 errors, 0 warnings
- **Ready for Testing**: End-to-end flow can be tested immediately

## Executive Summary
Implement a streamlined bidirectional feedback conversation system that enables flat discussions between participants and coordinators about form submissions. The system will feature inline feedback display, simple conversation threads, binary status management, and standard page-based updates, all integrated seamlessly with the existing Phoenix Admin Template design system.

## Problem Statement

### Current Limitations
1. **No Participant Visibility**: Participants cannot see feedback from coordinators on their submissions
2. **One-Way Communication**: Feedback flows only from coordinator to submission, with no response mechanism
3. **No Status Tracking**: No way to mark feedback as addressed or closed
4. **Missing Navigation**: No direct path from feedback notification to the specific form section

### User Pain Points

#### For Participants
- Cannot see what needs to be corrected in their submissions
- No way to respond to or clarify coordinator feedback
- Must guess what changes are needed when resubmitting
- Cannot mark issues as resolved after addressing them

#### For Coordinators
- Cannot see if participant has read or addressed feedback
- No way to have clarifying discussions with participants
- Must review entire form again to check if feedback was addressed
- No conversation history for audit trail

## Requirements

### Functional Requirements

#### FR-1: Flat Conversation Management
- **FR-1.1**: Support flat conversation threads (original feedback + sequential replies)
- **FR-1.2**: Allow both coordinators and participants to add replies
- **FR-1.3**: Display replies chronologically under original feedback
- **FR-1.4**: Show author identification and timestamps
- **FR-1.5**: No nested threading - all replies at same level

#### FR-2: Participant Dashboard Integration
- **FR-2.1**: Display feedback count widget with status indicators
- **FR-2.2**: List all active feedback with navigation links
- **FR-2.3**: Show feedback grouped by form and section
- **FR-2.4**: Provide quick actions (view, reply, close)
- **FR-2.5**: Visual distinction for new/unviewed feedback

#### FR-3: Form Editor Feedback Display
- **FR-3.1**: Show inline feedback beneath relevant questions/blocks
- **FR-3.2**: Visual indicators for questions with active feedback
- **FR-3.3**: Simple reply text box with send button
- **FR-3.4**: Close/reopen buttons based on current status
- **FR-3.5**: Load all feedback on page load (no real-time updates)

#### FR-4: Coordinator Review Enhancement
- **FR-4.1**: Display full conversation for each feedback item
- **FR-4.2**: Show participant responses inline
- **FR-4.3**: Visual indicators for review needed/closed status
- **FR-4.4**: Ability to reopen closed feedback
- **FR-4.5**: Reply capability for ongoing discussions

#### FR-5: Status Management
- **FR-5.1**: Two states only: Review Needed, Review Closed
- **FR-5.2**: Automatic reopen when participant replies to closed feedback
- **FR-5.3**: Manual close/reopen actions for both roles
- **FR-5.4**: Status persisted in database
- **FR-5.5**: Visual distinction between statuses

#### FR-6: Navigation and Deep Linking
- **FR-6.1**: Direct navigation from dashboard to specific form section
- **FR-6.2**: URL parameters for feedback context (e.g., ?feedbackId=123)
- **FR-6.3**: Auto-scroll to feedback location in form
- **FR-6.4**: Highlight animation for targeted feedback

### Non-Functional Requirements

#### NFR-1: Performance
- **NFR-1.1**: Page load with feedback < 500ms
- **NFR-1.2**: Reply submission < 500ms
- **NFR-1.3**: Standard browser caching for static content
- **NFR-1.4**: Efficient database queries with proper indexes

#### NFR-2: User Experience
- **NFR-2.1**: Phoenix-consistent visual design
- **NFR-2.2**: Mobile-responsive layout
- **NFR-2.3**: Clear visual feedback for actions
- **NFR-2.4**: Intuitive conversation flow

#### NFR-3: Security
- **NFR-3.1**: Role-based access control for feedback operations
- **NFR-3.2**: XSS prevention in feedback content
- **NFR-3.3**: CSRF protection for all actions
- **NFR-3.4**: Audit trail for all feedback actions

## Technical Design

### Database Schema Changes

#### Modified Tables

**ProjectFormFeedback**
```sql
ALTER TABLE [businessincubators].[ProjectFormFeedback]
ADD [ParentFeedbackId] BIGINT NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0=ReviewNeeded, 1=ReviewClosed
    [IsFromParticipant] BIT NOT NULL DEFAULT 0;

-- Foreign key for conversation grouping
ALTER TABLE [businessincubators].[ProjectFormFeedback]
ADD CONSTRAINT [FK_ProjectFormFeedback_ParentFeedback] 
FOREIGN KEY ([ParentFeedbackId]) 
REFERENCES [businessincubators].[ProjectFormFeedback]([Id]);

-- Index for performance
CREATE NONCLUSTERED INDEX [IX_ProjectFormFeedback_ParentFeedbackId]
ON [businessincubators].[ProjectFormFeedback]([ParentFeedbackId])
INCLUDE ([Status], [CreatedAt]);

CREATE NONCLUSTERED INDEX [IX_ProjectFormFeedback_Status_ReviewId]
ON [businessincubators].[ProjectFormFeedback]([Status], [ReviewId])
INCLUDE ([BlockId], [QuestionId], [IsFromParticipant]);
```

### Domain Model Updates

#### Enhanced ProjectFormFeedback Entity
```csharp
public class ProjectFormFeedback : Entity
{
    // Existing properties...
    
    // New properties
    public long? ParentFeedbackId { get; private set; }
    public FeedbackStatus Status { get; private set; }
    public bool IsFromParticipant { get; private set; }
    
    // Navigation properties
    public virtual ProjectFormFeedback? ParentFeedback { get; private set; }
    public virtual ICollection<ProjectFormFeedback> Replies { get; private set; }
    
    // Domain methods
    public ProjectFormFeedback Reply(
        string feedbackText, 
        string userId, 
        bool isFromParticipant)
    {
        if (ParentFeedbackId.HasValue)
            throw new InvalidOperationException("Cannot reply to a reply. Only original feedback can have replies.");
            
        var reply = new ProjectFormFeedback(
            ReviewId,
            BlockId,
            QuestionId,
            feedbackText,
            FeedbackType.Info)
        {
            ParentFeedbackId = Id,
            IsFromParticipant = isFromParticipant,
            Status = FeedbackStatus.ReviewNeeded,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        // Reopen if participant replies to closed feedback
        if (isFromParticipant && Status == FeedbackStatus.ReviewClosed)
        {
            Reopen(userId);
        }
        
        return reply;
    }
    
    public void Close(string userId)
    {
        if (Status == FeedbackStatus.ReviewClosed)
            return;
            
        Status = FeedbackStatus.ReviewClosed;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
    
    public void Reopen(string userId)
    {
        if (Status == FeedbackStatus.ReviewNeeded)
            return;
            
        Status = FeedbackStatus.ReviewNeeded;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}
```

#### New Enums
```csharp
public enum FeedbackStatus
{
    ReviewNeeded = 0,
    ReviewClosed = 1
}
```

### Application Layer

#### Commands

**ReplyToFeedbackCommand**
```csharp
public record ReplyToFeedbackCommand(
    long ParentFeedbackId,
    string FeedbackText,
    string UserId,
    bool IsFromParticipant
) : IBaseRequest<FeedbackDto>;

public class ReplyToFeedbackCommandHandler 
    : IBaseRequestHandler<ReplyToFeedbackCommand, FeedbackDto>
{
    public async Task<Result<FeedbackDto>> Handle(
        ReplyToFeedbackCommand request, 
        CancellationToken cancellationToken)
    {
        var parentFeedback = await _repository
            .GetFeedbackByIdAsync(request.ParentFeedbackId, cancellationToken);
            
        if (parentFeedback is null)
            return Result<FeedbackDto>.Failure("Feedback not found");
            
        var reply = parentFeedback.Reply(
            request.FeedbackText,
            request.UserId,
            request.IsFromParticipant);
            
        _repository.Add(reply);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<FeedbackDto>.Success(_mapper.Map<FeedbackDto>(reply));
    }
}
```

**CloseFeedbackCommand**
```csharp
public record CloseFeedbackCommand(
    long FeedbackId,
    string UserId
) : IBaseRequest<bool>;
```

**ReopenFeedbackCommand**
```csharp
public record ReopenFeedbackCommand(
    long FeedbackId,
    string UserId
) : IBaseRequest<bool>;
```

#### Queries

**GetFeedbackForSubmissionQuery**
```csharp
public record GetFeedbackForSubmissionQuery(
    long SubmissionId,
    string UserId
) : IBaseRequest<List<FeedbackConversationDto>>;

public class FeedbackConversationDto
{
    public long Id { get; set; }
    public long? BlockId { get; set; }
    public long? QuestionId { get; set; }
    public string BlockName { get; set; }
    public string? QuestionText { get; set; }
    public string FeedbackText { get; set; }
    public FeedbackType Type { get; set; }
    public FeedbackStatus Status { get; set; }
    public string AuthorName { get; set; }
    public bool IsFromParticipant { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FeedbackReplyDto> Replies { get; set; }
}

public class FeedbackReplyDto
{
    public long Id { get; set; }
    public string FeedbackText { get; set; }
    public string AuthorName { get; set; }
    public bool IsFromParticipant { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**GetPendingFeedbackCountQuery**
```csharp
public record GetPendingFeedbackCountQuery(
    long SubmissionId,
    string UserId
) : IBaseRequest<FeedbackCountDto>;

public class FeedbackCountDto
{
    public int ReviewNeededCount { get; set; }
    public int ReviewClosedCount { get; set; }
    public int NewFeedbackCount { get; set; }
}
```

### Web Layer Implementation

#### Participant Dashboard Enhancement

**Dashboard View Model**
```csharp
public class ProjectDashboardViewModel
{
    // Existing properties...
    
    // New feedback properties
    public int ReviewNeededCount { get; set; }
    public int ReviewClosedCount { get; set; }
    public List<FeedbackSummaryDto> RecentFeedback { get; set; }
    public bool HasNewFeedback { get; set; }
}

public class FeedbackSummaryDto
{
    public long Id { get; set; }
    public string Location { get; set; }
    public string Preview { get; set; }
    public FeedbackStatus Status { get; set; }
    public int ReplyCount { get; set; }
    public DateTime LastActivity { get; set; }
    public string NavigationUrl { get; set; }
}
```

**Dashboard View (Feedback Widget)**
```html
@if (Model.ReviewNeededCount > 0 || Model.ReviewClosedCount > 0)
{
    <div class="row mt-4">
        <div class="col-12">
            <div class="card phoenix-card">
                <div class="card-header bg-light d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="fas fa-comments text-warning me-2"></i>
                        Retroalimentación del Coordinador
                        @if (Model.HasNewFeedback)
                        {
                            <span class="badge bg-info ms-2">Nuevo</span>
                        }
                    </h5>
                    <div>
                        <span class="badge bg-warning">@Model.ReviewNeededCount Requiere Revisión</span>
                        <span class="badge bg-success ms-2">@Model.ReviewClosedCount Cerrados</span>
                    </div>
                </div>
                <div class="card-body">
                    @if (Model.RecentFeedback.Any())
                    {
                        <div class="feedback-list">
                            @foreach (var feedback in Model.RecentFeedback)
                            {
                                <div class="feedback-item d-flex align-items-start p-3 mb-2 
                                     @(feedback.Status == FeedbackStatus.ReviewClosed ? "closed" : "open")"
                                     data-feedback-id="@feedback.Id">
                                    
                                    <div class="feedback-icon me-3">
                                        @if (feedback.Status == FeedbackStatus.ReviewClosed)
                                        {
                                            <i class="fas fa-check-circle text-success fs-4"></i>
                                        }
                                        else
                                        {
                                            <i class="fas fa-exclamation-circle text-warning fs-4"></i>
                                        }
                                    </div>
                                    
                                    <div class="feedback-content flex-grow-1">
                                        <div class="d-flex justify-content-between align-items-start">
                                            <div>
                                                <h6 class="mb-1">@feedback.Location</h6>
                                                <p class="text-muted mb-2">@feedback.Preview...</p>
                                                <small class="text-muted">
                                                    <i class="far fa-clock"></i> @feedback.LastActivity.ToString("dd/MM/yyyy HH:mm")
                                                    @if (feedback.ReplyCount > 0)
                                                    {
                                                        <span class="ms-2">
                                                            <i class="far fa-comment"></i> @feedback.ReplyCount respuestas
                                                        </span>
                                                    }
                                                </small>
                                            </div>
                                            <a href="@feedback.NavigationUrl" 
                                               class="btn btn-sm btn-outline-primary">
                                                <i class="fas fa-arrow-right"></i> Ver
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            }
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-4">
                            <i class="fas fa-comments fa-3x text-muted mb-3"></i>
                            <p class="text-muted">No hay retroalimentación disponible</p>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
}
```

#### Form Editor Integration

**JavaScript Enhancement (participant-form.js)**
```javascript
class ParticipantFormManager {
    constructor() {
        // Existing properties...
        this.feedbackData = new Map();
        this.currentFeedbackId = null;
    }
    
    async initialize() {
        // Existing initialization...
        
        // Load feedback data if submission exists
        if (this.submissionId) {
            await this.loadFeedbackData();
        }
        
        // Check for feedback navigation
        const urlParams = new URLSearchParams(window.location.search);
        const feedbackId = urlParams.get('feedbackId');
        if (feedbackId) {
            this.currentFeedbackId = feedbackId;
            await this.navigateToFeedback(feedbackId);
        }
        
        // Initialize feedback event handlers
        this.initializeFeedbackEventHandlers();
    }
    
    async loadFeedbackData() {
        try {
            const response = await fetch(`/api/feedback/submission/${this.submissionId}`);
            const feedback = await response.json();
            
            // Group feedback by location
            feedback.forEach(item => {
                const key = item.questionId || `block_${item.blockId}`;
                if (!this.feedbackData.has(key)) {
                    this.feedbackData.set(key, []);
                }
                this.feedbackData.get(key).push(item);
            });
        } catch (error) {
            console.error('Error loading feedback:', error);
        }
    }
    
    renderQuestionWithFeedback(question, questionResponse) {
        let html = this.renderQuestion(question, questionResponse);
        
        // Check for feedback
        const feedbackList = this.feedbackData.get(question.questionId);
        if (feedbackList && feedbackList.length > 0) {
            // Add feedback display
            const feedbackHtml = this.renderFeedbackConversation(feedbackList, question.questionId);
            html = html.replace('</div><!-- end form-question -->', 
                feedbackHtml + '</div><!-- end form-question -->');
            
            // Add visual indicator
            const statusClass = feedbackList[0].status === 1 ? 'feedback-closed' : 'feedback-open';
            html = html.replace('class="form-question"', 
                `class="form-question has-feedback ${statusClass}"`);
        }
        
        return html;
    }
    
    renderFeedbackConversation(conversations, locationId) {
        // Only render original feedback (not replies)
        const originalFeedback = conversations.filter(f => !f.parentFeedbackId);
        
        return originalFeedback.map(feedback => `
            <div class="feedback-conversation mt-3" data-feedback-id="${feedback.id}" data-location-id="${locationId}">
                <div class="feedback-header d-flex justify-content-between align-items-center mb-2">
                    <div>
                        <i class="fas fa-comment-dots text-warning me-2"></i>
                        <span class="fw-bold">Retroalimentación</span>
                        ${this.renderFeedbackStatus(feedback.status)}
                    </div>
                </div>
                
                <div class="feedback-messages">
                    <!-- Original feedback -->
                    <div class="feedback-message coordinator mb-2">
                        <div class="feedback-author">
                            <strong>${feedback.authorName}</strong>
                            <small class="text-muted ms-2">${this.formatDate(feedback.createdAt)}</small>
                        </div>
                        <div class="feedback-content mt-1">${this.escapeHtml(feedback.feedbackText)}</div>
                    </div>
                    
                    <!-- Replies -->
                    ${feedback.replies ? feedback.replies.map(reply => `
                        <div class="feedback-message ${reply.isFromParticipant ? 'participant' : 'coordinator'} mb-2 ms-3">
                            <div class="feedback-author">
                                <strong>${reply.authorName}</strong>
                                <small class="text-muted ms-2">${this.formatDate(reply.createdAt)}</small>
                            </div>
                            <div class="feedback-content mt-1">${this.escapeHtml(reply.feedbackText)}</div>
                        </div>
                    `).join('') : ''}
                </div>
                
                <div class="feedback-actions mt-3">
                    <div class="input-group">
                        <input type="text" class="form-control reply-input" 
                               placeholder="Escribir respuesta..." 
                               data-feedback-id="${feedback.id}">
                        <button class="btn btn-primary send-reply" data-feedback-id="${feedback.id}">
                            <i class="fas fa-paper-plane"></i> Enviar
                        </button>
                        ${feedback.status === 0 ? `
                            <button class="btn btn-success close-feedback" data-feedback-id="${feedback.id}">
                                <i class="fas fa-check"></i> Cerrar
                            </button>
                        ` : `
                            <button class="btn btn-warning reopen-feedback" data-feedback-id="${feedback.id}">
                                <i class="fas fa-redo"></i> Reabrir
                            </button>
                        `}
                    </div>
                </div>
            </div>
        `).join('');
    }
    
    renderFeedbackStatus(status) {
        return status === 0 
            ? '<span class="badge bg-warning ms-2">Requiere Revisión</span>'
            : '<span class="badge bg-success ms-2">Cerrado</span>';
    }
    
    async navigateToFeedback(feedbackId) {
        // Find feedback location
        let targetLocation = null;
        for (const [key, feedbackList] of this.feedbackData) {
            if (feedbackList.some(f => f.id == feedbackId)) {
                targetLocation = key;
                break;
            }
        }
        
        if (!targetLocation) return;
        
        // Navigate to the block containing the feedback
        const isQuestion = !targetLocation.startsWith('block_');
        if (isQuestion) {
            // Find question's block
            for (const block of this.formData.blocks) {
                if (block.questions.some(q => q.questionId == targetLocation)) {
                    await this.navigateToBlock(this.formData.blocks.indexOf(block));
                    break;
                }
            }
        }
        
        // Scroll to and highlight feedback
        setTimeout(() => {
            const element = document.querySelector(`[data-feedback-id="${feedbackId}"]`);
            if (element) {
                element.scrollIntoView({ behavior: 'smooth', block: 'center' });
                element.classList.add('highlight-animation');
            }
        }, 500);
    }
    
    initializeFeedbackEventHandlers() {
        // Send reply
        document.addEventListener('click', async (e) => {
            if (e.target.closest('.send-reply')) {
                const btn = e.target.closest('.send-reply');
                const feedbackId = btn.dataset.feedbackId;
                const input = document.querySelector(`.reply-input[data-feedback-id="${feedbackId}"]`);
                
                if (input.value.trim()) {
                    await this.sendReply(feedbackId, input.value);
                    input.value = '';
                }
            }
        });
        
        // Close feedback
        document.addEventListener('click', async (e) => {
            if (e.target.closest('.close-feedback')) {
                const btn = e.target.closest('.close-feedback');
                const feedbackId = btn.dataset.feedbackId;
                
                if (confirm('¿Cerrar esta retroalimentación?')) {
                    await this.closeFeedback(feedbackId);
                }
            }
        });
        
        // Reopen feedback
        document.addEventListener('click', async (e) => {
            if (e.target.closest('.reopen-feedback')) {
                const btn = e.target.closest('.reopen-feedback');
                const feedbackId = btn.dataset.feedbackId;
                await this.reopenFeedback(feedbackId);
            }
        });
    }
    
    async sendReply(parentFeedbackId, text) {
        try {
            const response = await fetch('/api/feedback/reply', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': this.csrfToken
                },
                body: JSON.stringify({
                    parentFeedbackId: parentFeedbackId,
                    feedbackText: text,
                    isFromParticipant: true
                })
            });
            
            if (response.ok) {
                // Reload feedback to show new reply
                await this.loadFeedbackData();
                this.refreshCurrentBlock();
                
                window.toastManager.show({
                    title: 'Respuesta enviada',
                    message: 'Tu respuesta ha sido enviada al coordinador',
                    type: 'success'
                });
            }
        } catch (error) {
            console.error('Error sending reply:', error);
            window.toastManager.show({
                title: 'Error',
                message: 'No se pudo enviar la respuesta',
                type: 'error'
            });
        }
    }
    
    async closeFeedback(feedbackId) {
        try {
            const response = await fetch(`/api/feedback/${feedbackId}/close`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': this.csrfToken
                }
            });
            
            if (response.ok) {
                await this.loadFeedbackData();
                this.refreshCurrentBlock();
                
                window.toastManager.show({
                    title: 'Retroalimentación cerrada',
                    message: 'Se ha marcado como cerrada',
                    type: 'success'
                });
            }
        } catch (error) {
            console.error('Error closing feedback:', error);
        }
    }
    
    async reopenFeedback(feedbackId) {
        try {
            const response = await fetch(`/api/feedback/${feedbackId}/reopen`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': this.csrfToken
                }
            });
            
            if (response.ok) {
                await this.loadFeedbackData();
                this.refreshCurrentBlock();
                
                window.toastManager.show({
                    title: 'Retroalimentación reabierta',
                    message: 'Se ha marcado como requiere revisión',
                    type: 'info'
                });
            }
        } catch (error) {
            console.error('Error reopening feedback:', error);
        }
    }
}
```

**CSS Styles**
```css
/* Feedback Conversation Styles */
.feedback-conversation {
    background: var(--phoenix-gray-100);
    border-radius: 0.5rem;
    padding: 1rem;
    margin-top: 1rem;
    border: 1px solid var(--phoenix-gray-300);
}

.feedback-header {
    font-size: 0.875rem;
    color: var(--phoenix-gray-700);
}

.feedback-messages {
    margin-top: 1rem;
}

.feedback-message {
    background: white;
    padding: 0.75rem;
    border-radius: 0.375rem;
    border-left: 3px solid transparent;
}

.feedback-message.coordinator {
    background: var(--phoenix-warning-bg-subtle);
    border-left-color: var(--phoenix-warning);
}

.feedback-message.participant {
    background: var(--phoenix-primary-bg-subtle);
    border-left-color: var(--phoenix-primary);
}

/* Question with Feedback Indicators */
.form-question.has-feedback.feedback-open {
    border-left: 4px solid var(--phoenix-warning);
    background: linear-gradient(90deg, 
        var(--phoenix-warning-bg-subtle) 0%, 
        transparent 10%);
}

.form-question.has-feedback.feedback-closed {
    border-left: 4px solid var(--phoenix-success);
    background: linear-gradient(90deg, 
        var(--phoenix-success-bg-subtle) 0%, 
        transparent 10%);
}

/* Feedback Actions */
.feedback-actions .input-group {
    width: 100%;
}

/* Dashboard Feedback Widget */
.feedback-item {
    background: white;
    border: 1px solid var(--phoenix-gray-200);
    border-radius: 0.5rem;
    transition: all 0.3s ease;
    cursor: pointer;
}

.feedback-item:hover {
    border-color: var(--phoenix-primary);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.feedback-item.open {
    border-left: 4px solid var(--phoenix-warning);
}

.feedback-item.closed {
    opacity: 0.7;
    background: var(--phoenix-success-bg-subtle);
    border-left: 4px solid var(--phoenix-success);
}

/* Animations */
@keyframes highlight-animation {
    0%, 100% { background-color: transparent; }
    50% { background-color: var(--phoenix-info-bg-subtle); }
}

.highlight-animation {
    animation: highlight-animation 1s ease-in-out 3;
}

/* Mobile Responsive */
@media (max-width: 768px) {
    .feedback-conversation {
        padding: 0.75rem;
    }
    
    .feedback-actions .btn-group {
        flex-direction: column;
        width: 100%;
    }
    
    .feedback-actions .btn {
        width: 100%;
        margin-bottom: 0.5rem;
    }
}
```

### API Endpoints

```csharp
[ApiController]
[Route("api/feedback")]
[Authorize]
public class FeedbackApiController : ControllerBase
{
    [HttpGet("submission/{submissionId}")]
    public async Task<IActionResult> GetSubmissionFeedback(
        long submissionId, 
        CancellationToken cancellationToken)
    {
        var query = new GetFeedbackForSubmissionQuery(submissionId, User.GetUserId());
        var result = await _mediator.SendAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessages);
    }
    
    [HttpPost("reply")]
    public async Task<IActionResult> ReplyToFeedback(
        [FromBody] ReplyToFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReplyToFeedbackCommand(
            request.ParentFeedbackId,
            request.FeedbackText,
            User.GetUserId(),
            User.IsInRole(Roles.Participant));
            
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(new { success = true }) 
            : BadRequest(result.ErrorMessages);
    }
    
    [HttpPost("{feedbackId}/close")]
    public async Task<IActionResult> CloseFeedback(
        long feedbackId,
        CancellationToken cancellationToken)
    {
        var command = new CloseFeedbackCommand(feedbackId, User.GetUserId());
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(new { success = true }) 
            : BadRequest(result.ErrorMessages);
    }
    
    [HttpPost("{feedbackId}/reopen")]
    public async Task<IActionResult> ReopenFeedback(
        long feedbackId,
        CancellationToken cancellationToken)
    {
        var command = new ReopenFeedbackCommand(feedbackId, User.GetUserId());
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.IsSuccess ? Ok(new { success = true }) 
            : BadRequest(result.ErrorMessages);
    }
}
```

## Success Criteria

### Acceptance Criteria
1. ✅ Participants can see feedback count and status on dashboard
2. ✅ Clicking feedback navigates directly to form section
3. ✅ Feedback displays inline with questions/blocks
4. ✅ Participants can reply to coordinator feedback
5. ✅ Participants can close/reopen feedback
6. ✅ Coordinators see participant replies in review page
7. ✅ Flat conversation structure (no nested threads)
8. ✅ Two status states work correctly
9. ✅ Mobile layout is responsive
10. ✅ Page refresh shows updated feedback

### Performance Metrics
- Initial page load with feedback: < 500ms
- Reply submission: < 500ms
- Status change: < 300ms
- Navigation with feedback: < 1s

### Quality Metrics
- Zero console errors
- Clean build (0 errors, 0 warnings)
- Cross-browser support (Chrome, Firefox, Safari, Edge)

## Testing Strategy

### Unit Tests
- Domain entity methods (Reply, Close, Reopen)
- Command handlers validation
- Query handlers with filters

### Integration Tests
- Database operations with transactions
- API endpoint authorization
- Feedback navigation with deep linking

### Manual Testing Checklist
- [ ] Create feedback as coordinator
- [ ] View feedback as participant
- [ ] Reply to feedback (both roles)
- [ ] Close feedback
- [ ] Reopen feedback
- [ ] Navigate via feedback link
- [ ] Check mobile layout
- [ ] Verify status changes

## Implementation Timeline

### Phase 1: Backend (2 hours)
- Database schema changes (20 min)
- Domain model updates (30 min)
- Application layer commands/queries (45 min)
- API endpoints (25 min)

### Phase 2: Frontend (2.5 hours)
- Participant dashboard widget (45 min)
- Form editor integration (1.5 hours)
- Coordinator review integration (45 min)

### Phase 3: Polish (30 min)
- CSS styling and animations
- Mobile responsiveness
- Testing and bug fixes

**Total: 4.5-5 hours**

## Risk Mitigation

### Technical Risks
1. **Performance with many feedback items**
   - Mitigation: Proper database indexes and pagination if needed
   
2. **Concurrent updates**
   - Mitigation: Optimistic concurrency control in domain

### UX Risks
1. **Cluttered interface with many feedback items**
   - Mitigation: Clean, collapsed view by default
   
2. **Confusion about feedback status**
   - Mitigation: Clear visual indicators and simple two-state model

## Dependencies
- Existing feedback system (ProjectFormFeedback, ProjectFormReview)
- Phoenix Admin Template CSS variables
- Toast notification system (REQ-004)
- Form modernization (REQ-005)

## Related Documentation
- [Architecture Documentation](./../architecture.md)
- [DDD Patterns](./../ddd-patterns.md)
- [Common Issues](./../common-issues.md)
- [REQ-004: Toast Notifications](./completed/REQ-004-modern-toast-notifications.md)
- [REQ-005: Modern Form Experience](./completed/REQ-005-modern-form-experience.md)

## Notes
- System is not in production - no migration scripts needed
- Direct schema changes are allowed
- Follow existing DDD patterns strictly
- Maintain Phoenix Admin Template consistency
- All UI text must be in Spanish
- No real-time updates (SignalR) required
- No accessibility requirements for initial implementation
- Flat conversation structure (no nested threading)