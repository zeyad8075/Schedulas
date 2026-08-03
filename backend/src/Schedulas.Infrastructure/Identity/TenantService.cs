using Schedulas.Application.Common.Interfaces;

namespace Schedulas.Infrastructure.Identity;

public class TenantService : ITenantService
{
    private Guid? _tenantId;
    private bool _bypassIsolation;

    public TenantService(ICurrentUserService currentUserService)
    {
        // By default, a request inherits the tenant of the authenticated user
        _tenantId = currentUserService.InstitutionId;
    }

    public Guid? TenantId => _tenantId;
    public bool IsTenantEnforced => !_bypassIsolation;

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }

    public IDisposable BeginBypassScope()
    {
        return new BypassScope(this);
    }

    private class BypassScope : IDisposable
    {
        private readonly TenantService _tenantService;
        private readonly bool _originalBypassState;

        public BypassScope(TenantService tenantService)
        {
            _tenantService = tenantService;
            _originalBypassState = tenantService._bypassIsolation;
            
            // Activate bypass
            _tenantService._bypassIsolation = true;
        }

        public void Dispose()
        {
            // Restore previous state
            _tenantService._bypassIsolation = _originalBypassState;
        }
    }
}
