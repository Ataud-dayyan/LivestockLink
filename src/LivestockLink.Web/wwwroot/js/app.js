/**
 * LivestockLink Nigeria - B2B Portal Reactive Application
 */

let currentUser = null;
let currentView = '';

// Initialize Portal
document.addEventListener('DOMContentLoaded', async () => {
  currentUser = API.getCurrentUser();
  const token = API.getToken();

  if (!currentUser || !token) {
    window.location.href = '/index.html';
    return;
  }

  // Set user info in sidebar
  document.getElementById('user-display-name').innerText = currentUser.fullName;
  document.getElementById('user-display-role').innerText = currentUser.role;
  document.getElementById('user-display-profile').innerText = currentUser.profileName || currentUser.email;
  document.getElementById('portal-role-badge').innerText = currentUser.role;

  // Build sidebar navigation based on role
  buildSidebarNav(currentUser.role);

  // Set default view
  if (currentUser.role === 'Restaurant') {
    navigateTo('restaurant-catalogue');
  } else if (currentUser.role === 'Supplier') {
    navigateTo('supplier-dashboard');
  } else if (currentUser.role === 'Admin') {
    navigateTo('admin-dashboard');
  }
});

// Build Sidebar Navigation
function buildSidebarNav(role) {
  const navContainer = document.getElementById('sidebar-nav-items');
  navContainer.innerHTML = '';

  let links = [];

  if (role === 'Restaurant') {
    links = [
      { id: 'restaurant-catalogue', icon: '🛒', label: 'Product Catalogue' },
      { id: 'restaurant-dashboard', icon: '📊', label: 'Buyer Dashboard' },
      { id: 'restaurant-orders', icon: '📦', label: 'My Orders & Deliveries' }
    ];
  } else if (role === 'Supplier') {
    links = [
      { id: 'supplier-dashboard', icon: '📊', label: 'Supplier Dashboard' },
      { id: 'supplier-orders', icon: '📥', label: 'Incoming Orders' },
      { id: 'supplier-products', icon: '🌾', label: 'Manage Listings' }
    ];
  } else if (role === 'Admin') {
    links = [
      { id: 'admin-dashboard', icon: '📈', label: 'Platform Overview' },
      { id: 'admin-orders', icon: '📋', label: 'All Orders' },
      { id: 'admin-disputes', icon: '⚠️', label: 'Disputes Center' },
      { id: 'admin-suppliers', icon: '🚜', label: 'Suppliers Directory' },
      { id: 'admin-restaurants', icon: '🍽️', label: 'Restaurants Directory' }
    ];
  }

  links.forEach(item => {
    const btn = document.createElement('button');
    btn.className = 'sidebar-btn';
    btn.id = `nav-${item.id}`;
    btn.innerHTML = `<span>${item.icon}</span> <span>${item.label}</span>`;
    btn.onclick = () => navigateTo(item.id);
    navContainer.appendChild(btn);
  });
}

// Navigation Controller
function navigateTo(viewId) {
  currentView = viewId;

  // Update active sidebar button
  document.querySelectorAll('.sidebar-btn').forEach(btn => btn.classList.remove('active'));
  const activeBtn = document.getElementById(`nav-${viewId}`);
  if (activeBtn) activeBtn.classList.add('active');

  const container = document.getElementById('main-content');
  container.innerHTML = '<div style="text-align:center; padding: 3rem;"><div class="metric-label">Loading view...</div></div>';

  switch (viewId) {
    // Restaurant Views
    case 'restaurant-dashboard':
      renderRestaurantDashboard();
      break;
    case 'restaurant-catalogue':
      renderRestaurantCatalogue();
      break;
    case 'restaurant-orders':
      renderRestaurantOrders();
      break;

    // Supplier Views
    case 'supplier-dashboard':
      renderSupplierDashboard();
      break;
    case 'supplier-orders':
      renderSupplierOrders();
      break;
    case 'supplier-products':
      renderSupplierProducts();
      break;

    // Admin Views
    case 'admin-dashboard':
      renderAdminDashboard();
      break;
    case 'admin-orders':
      renderAdminOrders();
      break;
    case 'admin-disputes':
      renderAdminDisputes();
      break;
    case 'admin-suppliers':
      renderAdminSuppliers();
      break;
    case 'admin-restaurants':
      renderAdminRestaurants();
      break;

    default:
      container.innerHTML = '<div>View not found</div>';
  }
}

// ==========================================
// TOAST & MODAL HELPERS
// ==========================================
function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  const toast = document.createElement('div');
  toast.className = `toast ${type}`;
  toast.innerHTML = message;
  container.appendChild(toast);
  setTimeout(() => toast.remove(), 4000);
}

function openAppModal(title, contentHtml) {
  document.getElementById('app-modal-title').innerText = title;
  document.getElementById('app-modal-body').innerHTML = contentHtml;
  document.getElementById('app-modal').classList.add('active');
}

function closeAppModal() {
  document.getElementById('app-modal').classList.remove('active');
}

// Helper to switch demo accounts instantly
async function switchDemoAccount(email) {
  if (!email) return;
  try {
    const res = await API.post('/api/auth/login', {
      email: email,
      password: 'Password123!'
    });
    API.setToken(res.token);
    API.setCurrentUser(res.user);
    showToast(`Switched account to: ${res.user.fullName} (${res.user.role})`, 'success');
    setTimeout(() => window.location.reload(), 600);
  } catch (err) {
    showToast(`Failed to switch: ${err.message}`, 'error');
  }
}

// ==========================================
// RESTAURANT BUYER VIEWS
// ==========================================

// 1. Restaurant Dashboard
async function renderRestaurantDashboard() {
  const container = document.getElementById('main-content');
  try {
    const orders = await API.get('/api/orders/restaurant');

    const totalOrders = orders.length;
    const completedOrders = orders.filter(o => o.status === 'Completed');
    const totalSpent = completedOrders.reduce((sum, o) => sum + o.totalAmount, 0);
    const activeOrders = orders.filter(o => ['Submitted', 'Accepted', 'Dispatched'].includes(o.status));

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Restaurant Buyer Dashboard</h1>
          <div class="page-subtitle">Welcome back, ${currentUser.profileName || currentUser.fullName}</div>
        </div>
        <button class="btn btn-primary" onclick="navigateTo('restaurant-catalogue')">
          🛒 Browse Marketplace
        </button>
      </div>

      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-label">Active Orders</div>
          <div class="metric-value">${activeOrders.length}</div>
          <div class="metric-sub">In review, preparation, or transit</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Completed Deliveries</div>
          <div class="metric-value">${completedOrders.length}</div>
          <div class="metric-sub">Confirmed kitchen receipts</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Total Spend (Completed)</div>
          <div class="metric-value">${API.formatNaira(totalSpent)}</div>
          <div class="metric-sub">Across ${completedOrders.length} fulfilled orders</div>
        </div>
      </div>

      <div class="page-header" style="margin-top: 2rem;">
        <h2 style="font-size: 1.25rem; font-weight: 700;">Recent Order Requests</h2>
        <button class="btn btn-secondary btn-sm" onclick="navigateTo('restaurant-orders')">View All Orders</button>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Product</th>
                <th>Supplier</th>
                <th>Quantity</th>
                <th>Total Value</th>
                <th>Delivery Date</th>
                <th>Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              ${orders.length === 0 ? `
                <tr><td colspan="8" class="empty-state">No orders placed yet. Browse the catalogue to order!</td></tr>
              ` : orders.slice(0, 5).map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong></td>
                  <td>${o.productTitle}</td>
                  <td>${o.supplierName}</td>
                  <td>${o.quantity} ${o.unit}</td>
                  <td><strong>${API.formatNaira(o.totalAmount)}</strong></td>
                  <td>${API.formatDate(o.preferredDeliveryDate)}</td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">Details</button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

// 2. Restaurant Catalogue View
let currentCatalogueFilter = {
  category: '',
  search: '',
  form: '',
  sort: ''
};

async function renderRestaurantCatalogue() {
  const container = document.getElementById('main-content');
  container.innerHTML = `
    <div class="page-header">
      <div>
        <h1 class="page-title">Livestock & Protein Catalogue</h1>
        <div class="page-subtitle">Verified Nigerian poultry farms, catfish hatcheries, and meat processors</div>
      </div>
    </div>

    <!-- Search & Filter Controls -->
    <div class="filter-bar">
      <input type="text" id="catalogue-search" class="search-input" placeholder="Search broilers, catfish, beef cuts, goats..." value="${currentCatalogueFilter.search}">
      
      <select id="catalogue-category" class="filter-select">
        <option value="">All Categories</option>
        <option value="Poultry" ${currentCatalogueFilter.category === 'Poultry' ? 'selected' : ''}>Poultry (Broilers, Layers)</option>
        <option value="CattleBeef" ${currentCatalogueFilter.category === 'CattleBeef' ? 'selected' : ''}>Cattle / Beef</option>
        <option value="GoatsSheep" ${currentCatalogueFilter.category === 'GoatsSheep' ? 'selected' : ''}>Goats & Sheep (Ram)</option>
        <option value="CatfishFish" ${currentCatalogueFilter.category === 'CatfishFish' ? 'selected' : ''}>Catfish & Aquaculture</option>
        <option value="OtherLivestock" ${currentCatalogueFilter.category === 'OtherLivestock' ? 'selected' : ''}>Other Livestock</option>
      </select>

      <select id="catalogue-form" class="filter-select">
        <option value="">All Product Forms</option>
        <option value="LiveAnimal" ${currentCatalogueFilter.form === 'LiveAnimal' ? 'selected' : ''}>Live Animal</option>
        <option value="ProcessedMeat" ${currentCatalogueFilter.form === 'ProcessedMeat' ? 'selected' : ''}>Processed Meat</option>
        <option value="CleanedFish" ${currentCatalogueFilter.form === 'CleanedFish' ? 'selected' : ''}>Cleaned Fish</option>
        <option value="SmokedDried" ${currentCatalogueFilter.form === 'SmokedDried' ? 'selected' : ''}>Smoked / Dried</option>
        <option value="PackagedCuts" ${currentCatalogueFilter.form === 'PackagedCuts' ? 'selected' : ''}>Packaged Cuts</option>
      </select>

      <select id="catalogue-sort" class="filter-select">
        <option value="">Sort: Newest First</option>
        <option value="price_asc" ${currentCatalogueFilter.sort === 'price_asc' ? 'selected' : ''}>Price: Low to High</option>
        <option value="price_desc" ${currentCatalogueFilter.sort === 'price_desc' ? 'selected' : ''}>Price: High to Low</option>
        <option value="name" ${currentCatalogueFilter.sort === 'name' ? 'selected' : ''}>Alphabetical</option>
      </select>

      <button class="btn btn-primary btn-sm" onclick="applyCatalogueFilters()">Filter</button>
      <button class="btn btn-secondary btn-sm" onclick="resetCatalogueFilters()">Reset</button>
    </div>

    <!-- Product Grid -->
    <div id="product-grid-container" class="product-grid">
      <div style="grid-column: 1/-1; text-align: center; padding: 2rem;">Loading products...</div>
    </div>
  `;

  // Attach search on Enter key
  document.getElementById('catalogue-search').addEventListener('keyup', (e) => {
    if (e.key === 'Enter') applyCatalogueFilters();
  });

  await loadCatalogueProducts();
}

async function applyCatalogueFilters() {
  currentCatalogueFilter.search = document.getElementById('catalogue-search').value.trim();
  currentCatalogueFilter.category = document.getElementById('catalogue-category').value;
  currentCatalogueFilter.form = document.getElementById('catalogue-form').value;
  currentCatalogueFilter.sort = document.getElementById('catalogue-sort').value;
  await loadCatalogueProducts();
}

function resetCatalogueFilters() {
  currentCatalogueFilter = { category: '', search: '', form: '', sort: '' };
  document.getElementById('catalogue-search').value = '';
  document.getElementById('catalogue-category').value = '';
  document.getElementById('catalogue-form').value = '';
  document.getElementById('catalogue-sort').value = '';
  loadCatalogueProducts();
}

async function loadCatalogueProducts() {
  const grid = document.getElementById('product-grid-container');
  try {
    let queryParams = [];
    if (currentCatalogueFilter.category) queryParams.push(`Category=${currentCatalogueFilter.category}`);
    if (currentCatalogueFilter.form) queryParams.push(`ProductForm=${currentCatalogueFilter.form}`);
    if (currentCatalogueFilter.search) queryParams.push(`SearchQuery=${encodeURIComponent(currentCatalogueFilter.search)}`);
    if (currentCatalogueFilter.sort) queryParams.push(`SortBy=${currentCatalogueFilter.sort}`);

    const queryString = queryParams.length > 0 ? `?${queryParams.join('&')}` : '';
    const products = await API.get(`/api/products${queryString}`);

    if (products.length === 0) {
      grid.innerHTML = `
        <div style="grid-column: 1/-1;" class="empty-state">
          <div class="empty-icon">🔍</div>
          <div class="empty-title">No listings match your filter criteria</div>
          <div class="empty-desc">Try resetting filters or searching with different keywords.</div>
          <button class="btn btn-secondary btn-sm" onclick="resetCatalogueFilters()">Clear Filters</button>
        </div>
      `;
      return;
    }

    grid.innerHTML = products.map(p => `
      <div class="product-card">
        <img src="${p.imageUrl || 'https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=600'}" alt="${p.title}" class="product-img" onerror="this.src='https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=600'">
        <div class="product-body">
          <div class="product-meta">
            <span class="category-badge">${p.categoryName}</span>
            <span class="product-form-badge">${p.productFormName}</span>
          </div>
          <h3 class="product-title">${p.title}</h3>
          <div class="product-supplier">🌾 ${p.supplierName} (${p.supplierState})</div>
          <div class="product-price">
            ${API.formatNaira(p.pricePerUnit)} <span>/ ${p.unit}</span>
          </div>
          <div class="product-spec">📦 Available: <strong>${p.availableQuantity} ${p.unit}</strong></div>
          <div class="product-spec">⚡ Min. Order: <strong>${p.minimumOrderQuantity} ${p.unit}</strong></div>
          <div class="product-spec" style="font-size: 0.75rem; color: var(--slate-500); margin-top: 0.25rem;">
            📍 Coverage: ${p.deliveryCoverage}
          </div>
          <div class="product-actions">
            <button class="btn btn-primary" style="width: 100%;" onclick="openOrderModal('${p.id}')">
              Request Order
            </button>
          </div>
        </div>
      </div>
    `).join('');
  } catch (err) {
    grid.innerHTML = `<div style="grid-column: 1/-1;" class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

// 3. Product Details & Order Placement Modal
async function openOrderModal(productId) {
  try {
    const product = await API.get(`/api/products/${productId}`);
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const minDateStr = tomorrow.toISOString().split('T')[0];

    const contentHtml = `
      <div style="display: flex; gap: 1.25rem; margin-bottom: 1.25rem; flex-wrap: wrap;">
        <img src="${product.imageUrl || 'https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=600'}" style="width: 140px; height: 110px; object-fit: cover; border-radius: var(--radius-sm);" onerror="this.src='https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=600'">
        <div style="flex: 1;">
          <h4 style="font-size: 1.15rem; font-weight: 700; margin-bottom: 0.25rem;">${product.title}</h4>
          <div style="font-size: 0.85rem; color: var(--slate-600); margin-bottom: 0.25rem;">Supplier: <strong>${product.supplierName}</strong></div>
          <div style="font-size: 0.85rem; color: var(--slate-600); margin-bottom: 0.5rem;">Farm: ${product.supplierLocation}, ${product.supplierState}</div>
          <div style="font-size: 1.3rem; font-weight: 800; color: var(--primary);">
            ${API.formatNaira(product.pricePerUnit)} <span style="font-size: 0.85rem; font-weight: normal; color: var(--slate-500);">per ${product.unit}</span>
          </div>
        </div>
      </div>

      <div style="background: var(--slate-50); padding: 0.85rem; border-radius: var(--radius-sm); border: 1px solid var(--slate-200); margin-bottom: 1.25rem; font-size: 0.85rem; color: var(--slate-600);">
        ${product.description}
        <div style="margin-top: 0.5rem; font-weight: 600; color: var(--slate-700);">
          Delivery Coverage: ${product.deliveryCoverage}
        </div>
      </div>

      <form id="submit-order-form" onsubmit="handleOrderSubmission(event, '${product.id}', ${product.pricePerUnit})">
        <div class="form-row">
          <div class="form-col form-group">
            <label class="form-label">Order Quantity (${product.unit})</label>
            <input type="number" id="order-qty" class="form-control" 
              value="${product.minimumOrderQuantity}" 
              min="${product.minimumOrderQuantity}" 
              max="${product.availableQuantity}" 
              oninput="updateOrderCalculator(${product.pricePerUnit}, '${product.unit}')" required>
            <div class="form-text">Min: ${product.minimumOrderQuantity} | Max Available: ${product.availableQuantity} ${product.unit}</div>
          </div>
          <div class="form-col form-group">
            <label class="form-label">Preferred Delivery Date</label>
            <input type="date" id="order-date" class="form-control" min="${minDateStr}" value="${minDateStr}" required>
          </div>
        </div>

        <!-- Live Price Calculation Card -->
        <div style="background: #f0fdf4; border: 1px solid #bbf7d0; padding: 1rem; border-radius: var(--radius-sm); margin-bottom: 1.25rem; display: flex; justify-content: space-between; align-items: center;">
          <div>
            <div style="font-size: 0.75rem; text-transform: uppercase; color: var(--primary-dark); font-weight: 700;">Estimated Order Value</div>
            <div id="calc-breakdown" style="font-size: 0.85rem; color: var(--slate-600);">
              ${product.minimumOrderQuantity} ${product.unit} × ${API.formatNaira(product.pricePerUnit)}
            </div>
          </div>
          <div id="calc-total" style="font-size: 1.5rem; font-weight: 800; color: var(--primary-dark);">
            ${API.formatNaira(product.minimumOrderQuantity * product.pricePerUnit)}
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Delivery Address</label>
          <input type="text" id="order-address" class="form-control" placeholder="Kitchen delivery address" required>
        </div>

        <div class="form-row">
          <div class="form-col form-group">
            <label class="form-label">City</label>
            <input type="text" id="order-city" class="form-control" placeholder="e.g. Lagos / Abuja" required>
          </div>
          <div class="form-col form-group">
            <label class="form-label">State</label>
            <input type="text" id="order-state" class="form-control" placeholder="e.g. Lagos State" required>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Kitchen Notes / Special Requests (Optional)</label>
          <textarea id="order-notes" class="form-control" rows="2" placeholder="e.g. Delivery before 10 AM; please ensure crates are ventilated."></textarea>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem;">
          <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
          <button type="submit" class="btn btn-primary">Submit Order Request</button>
        </div>
      </form>
    `;

    openAppModal(`Place Order: ${product.title}`, contentHtml);
  } catch (err) {
    showToast(`Error opening order modal: ${err.message}`, 'error');
  }
}

function updateOrderCalculator(unitPrice, unit) {
  const qty = parseInt(document.getElementById('order-qty').value, 10) || 0;
  const total = qty * unitPrice;
  document.getElementById('calc-breakdown').innerText = `${qty} ${unit} × ${API.formatNaira(unitPrice)}`;
  document.getElementById('calc-total').innerText = API.formatNaira(total);
}

async function handleOrderSubmission(e, productId, unitPrice) {
  e.preventDefault();
  const qty = parseInt(document.getElementById('order-qty').value, 10);
  const deliveryDate = document.getElementById('order-date').value;
  const address = document.getElementById('order-address').value;
  const city = document.getElementById('order-city').value;
  const state = document.getElementById('order-state').value;
  const notes = document.getElementById('order-notes').value;

  try {
    const payload = {
      productId: productId,
      quantity: qty,
      preferredDeliveryDate: new Date(deliveryDate).toISOString(),
      deliveryAddress: address,
      deliveryCity: city,
      deliveryState: state,
      restaurantNotes: notes
    };

    const res = await API.post('/api/orders', payload);
    closeAppModal();
    showToast(`Order ${res.orderNumber} successfully submitted! Supplier will review.`, 'success');
    navigateTo('restaurant-orders');
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// 4. Restaurant Order History & Workflow View
async function renderRestaurantOrders() {
  const container = document.getElementById('main-content');
  try {
    const orders = await API.get('/api/orders/restaurant');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">My Orders & Deliveries</h1>
          <div class="page-subtitle">Track incoming shipments, confirm deliveries, and review completed orders</div>
        </div>
        <button class="btn btn-primary" onclick="navigateTo('restaurant-catalogue')">
          + Place New Order
        </button>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Item & Details</th>
                <th>Supplier</th>
                <th>Quantity</th>
                <th>Total Value</th>
                <th>Delivery Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              ${orders.length === 0 ? `
                <tr><td colspan="8" class="empty-state">No orders found. Click "Place New Order" to start!</td></tr>
              ` : orders.map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong><br><small style="color:var(--slate-400);">${API.formatDate(o.submittedAt)}</small></td>
                  <td><strong>${o.productTitle}</strong><br><small style="color:var(--slate-500);">${o.productForm}</small></td>
                  <td>${o.supplierName}</td>
                  <td>${o.quantity} ${o.unit}</td>
                  <td><strong>${API.formatNaira(o.totalAmount)}</strong></td>
                  <td>${API.formatDate(o.preferredDeliveryDate)}</td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <div style="display: flex; gap: 0.35rem; flex-wrap: wrap;">
                      <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">View</button>

                      ${o.status === 'Submitted' ? `
                        <button class="btn btn-danger btn-sm" onclick="openCancelModal('${o.id}')">Cancel</button>
                      ` : ''}

                      ${o.status === 'Dispatched' ? `
                        <button class="btn btn-success btn-sm" onclick="confirmDelivery('${o.id}')">Confirm Delivery</button>
                        <button class="btn btn-danger btn-sm" onclick="openDisputeModal('${o.id}')">Dispute</button>
                      ` : ''}
                    </div>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

// Order Details Modal (Shared across roles)
async function viewOrderDetails(orderId) {
  try {
    const order = await API.get(`/api/orders/${orderId}`);

    // Workflow steps progress
    const steps = ['Submitted', 'Accepted', 'Dispatched', 'Completed'];
    const currentIdx = steps.indexOf(order.status);

    const contentHtml = `
      <!-- Timeline Header -->
      <div class="timeline">
        <div class="timeline-step ${order.submittedAt ? 'completed' : ''}">
          <div class="timeline-dot">1</div>
          <div class="timeline-label">Submitted</div>
        </div>
        <div class="timeline-step ${order.acceptedAt ? 'completed' : (order.status === 'Submitted' ? 'active' : '')}">
          <div class="timeline-dot">2</div>
          <div class="timeline-label">Accepted</div>
        </div>
        <div class="timeline-step ${order.dispatchedAt ? 'completed' : (order.status === 'Accepted' ? 'active' : '')}">
          <div class="timeline-dot">3</div>
          <div class="timeline-label">Dispatched</div>
        </div>
        <div class="timeline-step ${order.completedAt ? 'completed' : (order.status === 'Dispatched' ? 'active' : '')}">
          <div class="timeline-dot">4</div>
          <div class="timeline-label">Completed</div>
        </div>
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1.25rem;">
        <div>
          <div style="font-size: 0.8rem; color: var(--slate-500); text-transform: uppercase; font-weight: 700;">Order Reference</div>
          <div style="font-size: 1.1rem; font-weight: 800;">${order.orderNumber}</div>
        </div>
        <div>
          <div style="font-size: 0.8rem; color: var(--slate-500); text-transform: uppercase; font-weight: 700;">Status</div>
          <div><span class="badge badge-${order.status.toLowerCase()}">${order.status}</span></div>
        </div>
      </div>

      <div class="table-card" style="margin-bottom: 1.25rem;">
        <table class="data-table">
          <tr>
            <th>Product</th>
            <td>${order.productTitle} (${order.productCategory} - ${order.productForm})</td>
          </tr>
          <tr>
            <th>Quantity</th>
            <td>${order.quantity} ${order.unit} @ ${API.formatNaira(order.unitPriceAtOrder)}</td>
          </tr>
          <tr>
            <th>Total Amount</th>
            <td><strong style="color: var(--primary); font-size: 1.1rem;">${API.formatNaira(order.totalAmount)}</strong></td>
          </tr>
          <tr>
            <th>Supplier</th>
            <td>${order.supplierName} (Phone: ${order.supplierPhone || 'N/A'})</td>
          </tr>
          <tr>
            <th>Buyer / Restaurant</th>
            <td>${order.restaurantName} (Contact: ${order.restaurantContactPerson})</td>
          </tr>
          <tr>
            <th>Delivery Location</th>
            <td>${order.deliveryAddress}, ${order.deliveryCity}, ${order.deliveryState}</td>
          </tr>
          <tr>
            <th>Preferred Date</th>
            <td>${API.formatDate(order.preferredDeliveryDate)}</td>
          </tr>
          ${order.restaurantNotes ? `<tr><th>Restaurant Notes</th><td>${order.restaurantNotes}</td></tr>` : ''}
          ${order.supplierNotes ? `<tr><th>Supplier Notes</th><td>${order.supplierNotes}</td></tr>` : ''}
          ${order.rejectionReason ? `<tr style="color: var(--danger);"><th>Rejection Reason</th><td>${order.rejectionReason}</td></tr>` : ''}
          ${order.cancellationReason ? `<tr style="color: var(--slate-500);"><th>Cancellation Reason</th><td>${order.cancellationReason}</td></tr>` : ''}
          ${order.dispatchNotes ? `
            <tr style="background: #faf5ff;">
              <th>Logistics / Dispatch</th>
              <td>
                <strong>Notes:</strong> ${order.dispatchNotes}<br>
                <strong>Courier / Driver:</strong> ${order.courierContact || 'N/A'}
              </td>
            </tr>
          ` : ''}
        </table>
      </div>

      <!-- Commission Section (Visible to Supplier and Admin) -->
      ${order.commission ? `
        <div style="background: #f0fdf4; border: 1px solid #bbf7d0; padding: 1rem; border-radius: var(--radius-sm); margin-bottom: 1.25rem;">
          <div style="font-size: 0.8rem; font-weight: 700; color: var(--primary-dark); text-transform: uppercase;">Brokerage & Commission Breakdown</div>
          <div style="display: flex; justify-content: space-between; margin-top: 0.5rem; font-size: 0.9rem;">
            <span>Gross Order Value:</span> <strong>${API.formatNaira(order.commission.totalOrderValue)}</strong>
          </div>
          <div style="display: flex; justify-content: space-between; font-size: 0.9rem; color: #b45309;">
            <span>Platform Commission (${order.commission.commissionPercentage}%):</span> <strong>- ${API.formatNaira(order.commission.commissionAmount)}</strong>
          </div>
          <div style="display: flex; justify-content: space-between; font-size: 1rem; color: var(--primary-dark); font-weight: 800; border-top: 1px dashed #86efac; padding-top: 0.5rem; margin-top: 0.5rem;">
            <span>Supplier Net Payout:</span> <strong>${API.formatNaira(order.commission.supplierNetPayout)}</strong>
          </div>
        </div>
      ` : ''}

      <!-- Dispute Section if applicable -->
      ${order.dispute ? `
        <div style="background: #fff1f2; border: 1px solid #fecdd3; padding: 1rem; border-radius: var(--radius-sm); margin-bottom: 1.25rem;">
          <div style="font-size: 0.8rem; font-weight: 700; color: var(--danger); text-transform: uppercase;">Active Dispute Filed</div>
          <div style="font-size: 0.9rem; margin-top: 0.25rem;"><strong>Reason:</strong> ${order.dispute.reason}</div>
          <div style="font-size: 0.85rem; color: var(--slate-600); margin-top: 0.25rem;">${order.dispute.description}</div>
          <div style="margin-top: 0.5rem;"><span class="badge badge-disputed">${order.dispute.status}</span></div>
          ${order.dispute.resolutionNotes ? `<div style="font-size: 0.85rem; margin-top: 0.5rem; color: #166534;"><strong>Resolution:</strong> ${order.dispute.resolutionNotes}</div>` : ''}
        </div>
      ` : ''}

      <div style="display: flex; justify-content: flex-end; gap: 0.5rem;">
        <button class="btn btn-secondary" onclick="closeAppModal()">Close</button>
      </div>
    `;

    openAppModal(`Order Details: ${order.orderNumber}`, contentHtml);
  } catch (err) {
    showToast(`Error fetching order details: ${err.message}`, 'error');
  }
}

// Confirm Delivery Action (Restaurant)
async function confirmDelivery(orderId) {
  if (!confirm('Have you physically received and inspected the livestock/goods in good condition at your kitchen? Confirming delivery completes the order.')) {
    return;
  }

  try {
    const res = await API.post(`/api/orders/${orderId}/confirm-delivery`, {});
    showToast(`Delivery confirmed for ${res.orderNumber}! Platform commission calculated.`, 'success');
    renderRestaurantOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// Cancel Order Modal (Restaurant)
function openCancelModal(orderId) {
  const contentHtml = `
    <form onsubmit="handleCancelSubmit(event, '${orderId}')">
      <div class="form-group">
        <label class="form-label">Reason for Cancellation</label>
        <textarea id="cancel-reason" class="form-control" rows="3" placeholder="e.g. Menu changes, inventory restocked from partner kitchen" required></textarea>
      </div>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Back</button>
        <button type="submit" class="btn btn-danger">Confirm Cancellation</button>
      </div>
    </form>
  `;
  openAppModal('Cancel Order Request', contentHtml);
}

async function handleCancelSubmit(e, orderId) {
  e.preventDefault();
  const reason = document.getElementById('cancel-reason').value;
  try {
    await API.post(`/api/orders/${orderId}/cancel`, { reason });
    closeAppModal();
    showToast('Order successfully cancelled. Product stock has been restored.', 'success');
    renderRestaurantOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// Raise Dispute Modal (Restaurant)
function openDisputeModal(orderId) {
  const contentHtml = `
    <form onsubmit="handleDisputeSubmit(event, '${orderId}')">
      <div class="form-group">
        <label class="form-label">Dispute Reason</label>
        <select id="dispute-reason" class="form-control" required>
          <option value="Damaged Cargo / Mortality in transit">Damaged Cargo / High mortality during transit</option>
          <option value="Short Quantity Delivered">Delivered quantity less than ordered quantity</option>
          <option value="Spoiled or Off-Spec Meat">Spoiled or off-specification product form</option>
          <option value="Non-Delivery">Driver did not arrive at designated delivery address</option>
          <option value="Other Quality Concern">Other quality concern</option>
        </select>
      </div>
      <div class="form-group">
        <label class="form-label">Detailed Description of Issue</label>
        <textarea id="dispute-desc" class="form-control" rows="3" placeholder="Provide exact counts, driver remarks, and kitchen condition on arrival" required></textarea>
      </div>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Close</button>
        <button type="submit" class="btn btn-danger">Submit Dispute to Admin</button>
      </div>
    </form>
  `;
  openAppModal('Raise Order Delivery Dispute', contentHtml);
}

async function handleDisputeSubmit(e, orderId) {
  e.preventDefault();
  const reason = document.getElementById('dispute-reason').value;
  const description = document.getElementById('dispute-desc').value;
  try {
    await API.post(`/api/disputes/${orderId}`, { reason, description });
    closeAppModal();
    showToast('Dispute submitted. Platform Administrator will review and resolve.', 'warning');
    renderRestaurantOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ==========================================
// SUPPLIER VIEWS
// ==========================================

// 1. Supplier Dashboard & Revenue
async function renderSupplierDashboard() {
  const container = document.getElementById('main-content');
  try {
    const orders = await API.get('/api/orders/supplier');

    const completed = orders.filter(o => o.status === 'Completed');
    const grossSales = completed.reduce((sum, o) => sum + o.totalAmount, 0);
    const commissionDeducted = completed.reduce((sum, o) => sum + (o.commission ? o.commission.commissionAmount : 0), 0);
    const netEarnings = grossSales - commissionDeducted;

    const pendingRequests = orders.filter(o => o.status === 'Submitted');
    const activeDispatched = orders.filter(o => o.status === 'Dispatched');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Supplier Operations & Revenue</h1>
          <div class="page-subtitle">${currentUser.profileName || currentUser.fullName} — Livestock & Agro Fulfillment</div>
        </div>
        <div style="display: flex; gap: 0.5rem;">
          <button class="btn btn-primary" onclick="openAddProductModal()">+ Add New Listing</button>
          <button class="btn btn-secondary" onclick="navigateTo('supplier-orders')">View Orders Queue</button>
        </div>
      </div>

      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-label">Gross Completed Sales</div>
          <div class="metric-value" style="color: var(--primary);">${API.formatNaira(grossSales)}</div>
          <div class="metric-sub">${completed.length} successfully delivered orders</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Platform Commissions</div>
          <div class="metric-value" style="color: #b45309;">- ${API.formatNaira(commissionDeducted)}</div>
          <div class="metric-sub">Standard 5.0% brokerage deduction</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Net Supplier Earnings</div>
          <div class="metric-value" style="color: #166534;">${API.formatNaira(netEarnings)}</div>
          <div class="metric-sub">Direct net payable to farm</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Pending Action</div>
          <div class="metric-value">${pendingRequests.length}</div>
          <div class="metric-sub">${pendingRequests.length} incoming requests awaiting review</div>
        </div>
      </div>

      <div class="page-header" style="margin-top: 2rem;">
        <h2 style="font-size: 1.25rem; font-weight: 700;">Recent Order Activity</h2>
        <button class="btn btn-secondary btn-sm" onclick="navigateTo('supplier-orders')">View All Incoming</button>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Restaurant Buyer</th>
                <th>Item Ordered</th>
                <th>Quantity</th>
                <th>Total Value</th>
                <th>Net Payout</th>
                <th>Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              ${orders.length === 0 ? `
                <tr><td colspan="8" class="empty-state">No orders received yet. Make sure your product listings are active!</td></tr>
              ` : orders.slice(0, 5).map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong></td>
                  <td><strong>${o.restaurantName}</strong><br><small style="color:var(--slate-500);">${o.deliveryCity}, ${o.deliveryState}</small></td>
                  <td>${o.productTitle}</td>
                  <td>${o.quantity} ${o.unit}</td>
                  <td>${API.formatNaira(o.totalAmount)}</td>
                  <td><strong style="color: #166534;">${o.commission ? API.formatNaira(o.commission.supplierNetPayout) : 'Est: ' + API.formatNaira(o.totalAmount * 0.95)}</strong></td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">View</button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

// 2. Supplier Product Listings Management
async function renderSupplierProducts() {
  const container = document.getElementById('main-content');
  try {
    const products = await API.get('/api/products/my-listings');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Manage Product Listings</h1>
          <div class="page-subtitle">Configure pricing, available stock levels, minimum order quantities, and coverage</div>
        </div>
        <button class="btn btn-primary" onclick="openAddProductModal()">+ Add New Product Listing</button>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Category</th>
                <th>Product Form</th>
                <th>Price per Unit</th>
                <th>Available Stock</th>
                <th>Min. Order</th>
                <th>Coverage</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              ${products.length === 0 ? `
                <tr><td colspan="9" class="empty-state">No product listings found. Click "+ Add New Product Listing" to start selling!</td></tr>
              ` : products.map(p => `
                <tr>
                  <td>
                    <div style="display: flex; align-items: center; gap: 0.75rem;">
                      <img src="${p.imageUrl || 'https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=100'}" style="width: 44px; height: 44px; border-radius: var(--radius-sm); object-fit: cover;" onerror="this.src='https://images.unsplash.com/photo-1548550023-2bdb3c5beed7?w=100'">
                      <div>
                        <strong>${p.title}</strong>
                      </div>
                    </div>
                  </td>
                  <td><span class="category-badge">${p.categoryName}</span></td>
                  <td><span class="product-form-badge">${p.productFormName}</span></td>
                  <td><strong>${API.formatNaira(p.pricePerUnit)}</strong> / ${p.unit}</td>
                  <td>${p.availableQuantity} ${p.unit}</td>
                  <td>${p.minimumOrderQuantity} ${p.unit}</td>
                  <td><small>${p.deliveryCoverage}</small></td>
                  <td>
                    <span class="badge ${p.isActive ? 'badge-completed' : 'badge-cancelled'}">
                      ${p.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    <div style="display: flex; gap: 0.35rem;">
                      <button class="btn btn-secondary btn-sm" onclick="toggleProductActive('${p.id}')">
                        ${p.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                      <button class="btn btn-secondary btn-sm" onclick="openEditProductModal('${p.id}')">Edit</button>
                    </div>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

async function toggleProductActive(productId) {
  try {
    await API.patch(`/api/products/${productId}/toggle-active`);
    showToast('Product status updated.', 'success');
    renderSupplierProducts();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// Add Product Modal
function openAddProductModal() {
  const contentHtml = `
    <form onsubmit="handleAddProductSubmit(event)">
      <div class="form-group">
        <label class="form-label">Product Title</label>
        <input type="text" id="prod-title" class="form-control" placeholder="e.g. Farm-Fresh Broiler Birds (Live)" required>
      </div>

      <div class="form-row">
        <div class="form-col form-group">
          <label class="form-label">Category</label>
          <select id="prod-category" class="form-control" required>
            <option value="Poultry">Poultry</option>
            <option value="CattleBeef">Cattle / Beef</option>
            <option value="GoatsSheep">Goats & Sheep</option>
            <option value="CatfishFish">Catfish & Aquaculture</option>
            <option value="OtherLivestock">Other Animal Protein</option>
          </select>
        </div>
        <div class="form-col form-group">
          <label class="form-label">Product Form</label>
          <select id="prod-form" class="form-control" required>
            <option value="LiveAnimal">Live Animal</option>
            <option value="ProcessedMeat">Processed Meat</option>
            <option value="CleanedFish">Cleaned / Gutted Fish</option>
            <option value="SmokedDried">Smoked / Dried</option>
            <option value="PackagedCuts">Packaged Cuts</option>
          </select>
        </div>
      </div>

      <div class="form-row">
        <div class="form-col form-group">
          <label class="form-label">Pricing Unit</label>
          <input type="text" id="prod-unit" class="form-control" placeholder="e.g. Bird, Kg, Head, Bag, Crate" required>
        </div>
        <div class="form-col form-group">
          <label class="form-label">Price per Unit (₦)</label>
          <input type="number" id="prod-price" class="form-control" placeholder="e.g. 8000" min="1" step="1" required>
        </div>
      </div>

      <div class="form-row">
        <div class="form-col form-group">
          <label class="form-label">Available Stock</label>
          <input type="number" id="prod-stock" class="form-control" placeholder="e.g. 200" min="0" required>
        </div>
        <div class="form-col form-group">
          <label class="form-label">Minimum Order Qty</label>
          <input type="number" id="prod-minorder" class="form-control" placeholder="e.g. 10" min="1" value="1" required>
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Delivery Coverage Areas</label>
        <input type="text" id="prod-coverage" class="form-control" placeholder="e.g. Lagos Mainland, Lagos Island, Ibadan, Ogun State" required>
      </div>

      <div class="form-group">
        <label class="form-label">Image URL (Optional)</label>
        <input type="url" id="prod-image" class="form-control" placeholder="https://images.unsplash.com/...">
      </div>

      <div class="form-group">
        <label class="form-label">Description & Quality Specifications</label>
        <textarea id="prod-desc" class="form-control" rows="3" placeholder="Specify age, average weight, feeding regime, cold-chain packaging..."></textarea>
      </div>

      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
        <button type="submit" class="btn btn-primary">Create Listing</button>
      </div>
    </form>
  `;

  openAppModal('Add New Livestock Product Listing', contentHtml);
}

async function handleAddProductSubmit(e) {
  e.preventDefault();
  const payload = {
    title: document.getElementById('prod-title').value.trim(),
    category: document.getElementById('prod-category').value,
    productForm: document.getElementById('prod-form').value,
    unit: document.getElementById('prod-unit').value.trim(),
    pricePerUnit: parseFloat(document.getElementById('prod-price').value),
    availableQuantity: parseInt(document.getElementById('prod-stock').value, 10),
    minimumOrderQuantity: parseInt(document.getElementById('prod-minorder').value, 10),
    deliveryCoverage: document.getElementById('prod-coverage').value.trim(),
    imageUrl: document.getElementById('prod-image').value.trim(),
    description: document.getElementById('prod-desc').value.trim()
  };

  try {
    await API.post('/api/products', payload);
    closeAppModal();
    showToast('Product listing published successfully!', 'success');
    renderSupplierProducts();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// Edit Product Modal
async function openEditProductModal(productId) {
  try {
    const p = await API.get(`/api/products/${productId}`);

    const contentHtml = `
      <form onsubmit="handleEditProductSubmit(event, '${productId}')">
        <div class="form-group">
          <label class="form-label">Product Title</label>
          <input type="text" id="edit-title" class="form-control" value="${p.title}" required>
        </div>

        <div class="form-row">
          <div class="form-col form-group">
            <label class="form-label">Category</label>
            <select id="edit-category" class="form-control" required>
              <option value="Poultry" ${p.category === 'Poultry' ? 'selected' : ''}>Poultry</option>
              <option value="CattleBeef" ${p.category === 'CattleBeef' ? 'selected' : ''}>Cattle / Beef</option>
              <option value="GoatsSheep" ${p.category === 'GoatsSheep' ? 'selected' : ''}>Goats & Sheep</option>
              <option value="CatfishFish" ${p.category === 'CatfishFish' ? 'selected' : ''}>Catfish & Aquaculture</option>
              <option value="OtherLivestock" ${p.category === 'OtherLivestock' ? 'selected' : ''}>Other Animal Protein</option>
            </select>
          </div>
          <div class="form-col form-group">
            <label class="form-label">Product Form</label>
            <select id="edit-form" class="form-control" required>
              <option value="LiveAnimal" ${p.productForm === 'LiveAnimal' ? 'selected' : ''}>Live Animal</option>
              <option value="ProcessedMeat" ${p.productForm === 'ProcessedMeat' ? 'selected' : ''}>Processed Meat</option>
              <option value="CleanedFish" ${p.productForm === 'CleanedFish' ? 'selected' : ''}>Cleaned Fish</option>
              <option value="SmokedDried" ${p.productForm === 'SmokedDried' ? 'selected' : ''}>Smoked / Dried</option>
              <option value="PackagedCuts" ${p.productForm === 'PackagedCuts' ? 'selected' : ''}>Packaged Cuts</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="form-col form-group">
            <label class="form-label">Pricing Unit</label>
            <input type="text" id="edit-unit" class="form-control" value="${p.unit}" required>
          </div>
          <div class="form-col form-group">
            <label class="form-label">Price per Unit (₦)</label>
            <input type="number" id="edit-price" class="form-control" value="${p.pricePerUnit}" min="1" step="1" required>
          </div>
        </div>

        <div class="form-row">
          <div class="form-col form-group">
            <label class="form-label">Available Stock</label>
            <input type="number" id="edit-stock" class="form-control" value="${p.availableQuantity}" min="0" required>
          </div>
          <div class="form-col form-group">
            <label class="form-label">Min. Order Qty</label>
            <input type="number" id="edit-minorder" class="form-control" value="${p.minimumOrderQuantity}" min="1" required>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Delivery Coverage</label>
          <input type="text" id="edit-coverage" class="form-control" value="${p.deliveryCoverage}" required>
        </div>

        <div class="form-group">
          <label class="form-label">Description</label>
          <textarea id="edit-desc" class="form-control" rows="3">${p.description || ''}</textarea>
        </div>

        <div class="form-group">
          <label style="display: flex; align-items: center; gap: 0.5rem; cursor: pointer;">
            <input type="checkbox" id="edit-active" ${p.isActive ? 'checked' : ''}>
            <span style="font-size: 0.9rem; font-weight: 600;">Listing is Active and Available to Buyers</span>
          </label>
        </div>

        <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
          <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
          <button type="submit" class="btn btn-primary">Save Changes</button>
        </div>
      </form>
    `;

    openAppModal(`Edit Listing: ${p.title}`, contentHtml);
  } catch (err) {
    showToast(err.message, 'error');
  }
}

async function handleEditProductSubmit(e, productId) {
  e.preventDefault();
  const payload = {
    title: document.getElementById('edit-title').value.trim(),
    category: document.getElementById('edit-category').value,
    productForm: document.getElementById('edit-form').value,
    unit: document.getElementById('edit-unit').value.trim(),
    pricePerUnit: parseFloat(document.getElementById('edit-price').value),
    availableQuantity: parseInt(document.getElementById('edit-stock').value, 10),
    minimumOrderQuantity: parseInt(document.getElementById('edit-minorder').value, 10),
    deliveryCoverage: document.getElementById('edit-coverage').value.trim(),
    description: document.getElementById('edit-desc').value.trim(),
    isActive: document.getElementById('edit-active').checked
  };

  try {
    await API.put(`/api/products/${productId}`, payload);
    closeAppModal();
    showToast('Listing updated successfully.', 'success');
    renderSupplierProducts();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// 3. Supplier Incoming Orders Queue
async function renderSupplierOrders() {
  const container = document.getElementById('main-content');
  try {
    const orders = await API.get('/api/orders/supplier');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Incoming Order Requests</h1>
          <div class="page-subtitle">Review, accept, dispatch, and track orders from restaurant kitchens</div>
        </div>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Restaurant Buyer</th>
                <th>Item Ordered</th>
                <th>Quantity</th>
                <th>Gross Total</th>
                <th>Delivery Date & Destination</th>
                <th>Status</th>
                <th>Fulfillment Action</th>
              </tr>
            </thead>
            <tbody>
              ${orders.length === 0 ? `
                <tr><td colspan="8" class="empty-state">No incoming orders in your queue.</td></tr>
              ` : orders.map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong><br><small style="color:var(--slate-400);">${API.formatDate(o.submittedAt)}</small></td>
                  <td><strong>${o.productTitle}</strong></td>
                  <td>${o.restaurantName}<br><small style="color:var(--slate-500);">${o.restaurantPhone || ''}</small></td>
                  <td>${o.quantity} ${o.unit}</td>
                  <td><strong>${API.formatNaira(o.totalAmount)}</strong></td>
                  <td>${API.formatDate(o.preferredDeliveryDate)}<br><small style="color:var(--slate-500);">${o.deliveryCity}, ${o.deliveryState}</small></td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <div style="display: flex; gap: 0.35rem; flex-wrap: wrap;">
                      <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">View</button>

                      ${o.status === 'Submitted' ? `
                        <button class="btn btn-primary btn-sm" onclick="acceptOrder('${o.id}')">Accept</button>
                        <button class="btn btn-danger btn-sm" onclick="openRejectModal('${o.id}')">Reject</button>
                      ` : ''}

                      ${o.status === 'Accepted' ? `
                        <button class="btn btn-success btn-sm" onclick="openDispatchModal('${o.id}')">Dispatch</button>
                      ` : ''}

                      ${o.status === 'Dispatched' ? `
                        <span style="font-size:0.75rem; color:#6d28d9; font-weight:600;">En Route</span>
                      ` : ''}

                      ${o.status === 'Completed' ? `
                        <span style="font-size:0.75rem; color:#15803d; font-weight:700;">Delivered</span>
                      ` : ''}
                    </div>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

async function acceptOrder(orderId) {
  try {
    await API.post(`/api/orders/${orderId}/accept`, { supplierNotes: "Order accepted. Cages/packaging ready." });
    showToast('Order accepted! You can now prepare and dispatch.', 'success');
    renderSupplierOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

function openRejectModal(orderId) {
  const contentHtml = `
    <form onsubmit="handleRejectSubmit(event, '${orderId}')">
      <div class="form-group">
        <label class="form-label">Rejection Reason</label>
        <textarea id="reject-reason" class="form-control" rows="3" placeholder="e.g. Stock reserved for existing contract; delivery distance outside today's logistics route" required></textarea>
      </div>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
        <button type="submit" class="btn btn-danger">Reject Order</button>
      </div>
    </form>
  `;
  openAppModal('Reject Order Request', contentHtml);
}

async function handleRejectSubmit(e, orderId) {
  e.preventDefault();
  const reason = document.getElementById('reject-reason').value;
  try {
    await API.post(`/api/orders/${orderId}/reject`, { reason });
    closeAppModal();
    showToast('Order rejected. Product stock restored.', 'info');
    renderSupplierOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

function openDispatchModal(orderId) {
  const contentHtml = `
    <form onsubmit="handleDispatchSubmit(event, '${orderId}')">
      <div class="form-group">
        <label class="form-label">Dispatch / Logistics Description</label>
        <input type="text" id="dispatch-notes" class="form-control" placeholder="e.g. Dispatched in refrigerated agro van LAG-442-XY" required>
      </div>
      <div class="form-group">
        <label class="form-label">Courier / Driver Contact Info</label>
        <input type="text" id="dispatch-courier" class="form-control" placeholder="e.g. Driver Sunday (+2348021112233)" required>
      </div>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
        <button type="submit" class="btn btn-success">Confirm Dispatch</button>
      </div>
    </form>
  `;
  openAppModal('Mark Order as Dispatched', contentHtml);
}

async function handleDispatchSubmit(e, orderId) {
  e.preventDefault();
  const dispatchNotes = document.getElementById('dispatch-notes').value;
  const courierContact = document.getElementById('dispatch-courier').value;

  try {
    await API.post(`/api/orders/${orderId}/dispatch`, { dispatchNotes, courierContact });
    closeAppModal();
    showToast('Order marked as dispatched! The restaurant has been notified for kitchen receipt.', 'success');
    renderSupplierOrders();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ==========================================
// ADMINISTRATOR / PLATFORM OWNER VIEWS
// ==========================================

// 1. Admin Platform Overview & Commission Settings
async function renderAdminDashboard() {
  const container = document.getElementById('main-content');
  try {
    const metrics = await API.get('/api/admin/metrics');
    const recentOrders = await API.get('/api/orders/all');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Platform Owner Dashboard</h1>
          <div class="page-subtitle">LivestockLink Nigeria — Marketplace Performance & Brokerage Revenue</div>
        </div>
      </div>

      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-label">Total Transaction Volume (GMV)</div>
          <div class="metric-value" style="color: var(--primary-dark);">${API.formatNaira(metrics.totalTransactionVolume)}</div>
          <div class="metric-sub">${metrics.completedOrdersCount} completed livestock transactions</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Commission Revenue Earned</div>
          <div class="metric-value" style="color: #166534;">${API.formatNaira(metrics.totalCommissionRevenue)}</div>
          <div class="metric-sub">Platform commission generated</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Total Orders Handled</div>
          <div class="metric-value">${metrics.totalOrdersCount}</div>
          <div class="metric-sub">${metrics.pendingOrdersCount} pending, ${metrics.dispatchedOrdersCount} in transit</div>
        </div>
        <div class="metric-card">
          <div class="metric-label">Disputed Orders</div>
          <div class="metric-value" style="color: ${metrics.disputedOrdersCount > 0 ? 'var(--danger)' : 'var(--slate-900)'};">${metrics.disputedOrdersCount}</div>
          <div class="metric-sub"><a href="javascript:void(0)" onclick="navigateTo('admin-disputes')">Review disputes &rarr;</a></div>
        </div>
      </div>

      <!-- Commission Rate Configuration Card -->
      <div style="background: var(--white); border: 1px solid var(--slate-200); border-radius: var(--radius-md); padding: 1.5rem; margin-bottom: 2rem;">
        <div style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem;">
          <div>
            <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--slate-900);">Platform Brokerage Commission Rate</h3>
            <p style="font-size: 0.85rem; color: var(--slate-500); margin-top: 0.2rem;">
              Currently configured commission percentage charged on completed livestock deliveries.
            </p>
          </div>
          <form onsubmit="handleUpdateCommissionRate(event)" style="display: flex; align-items: center; gap: 0.5rem;">
            <div style="display: flex; align-items: center; gap: 0.25rem;">
              <input type="number" id="admin-comm-rate" class="form-control" style="width: 100px;" value="${metrics.currentCommissionRate}" step="0.1" min="0.1" max="50" required>
              <span style="font-weight: 700; color: var(--slate-700);">%</span>
            </div>
            <button type="submit" class="btn btn-primary btn-sm">Update Rate</button>
          </form>
        </div>
      </div>

      <div class="page-header">
        <h2 style="font-size: 1.25rem; font-weight: 700;">Live Brokerage Transactions</h2>
        <button class="btn btn-secondary btn-sm" onclick="navigateTo('admin-orders')">View All (${recentOrders.length})</button>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Restaurant Buyer</th>
                <th>Supplier Farm</th>
                <th>Item Ordered</th>
                <th>Total Value</th>
                <th>Commission</th>
                <th>Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              ${recentOrders.slice(0, 6).map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong></td>
                  <td>${o.restaurantName}</td>
                  <td>${o.supplierName}</td>
                  <td>${o.productTitle} (${o.quantity} ${o.unit})</td>
                  <td>${API.formatNaira(o.totalAmount)}</td>
                  <td><strong style="color: #166534;">${o.commission ? API.formatNaira(o.commission.commissionAmount) : 'Pending'}</strong></td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">Inspect</button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

async function handleUpdateCommissionRate(e) {
  e.preventDefault();
  const rate = parseFloat(document.getElementById('admin-comm-rate').value);
  try {
    await API.put('/api/admin/commission-rate', { commissionPercentage: rate });
    showToast(`Commission rate successfully updated to ${rate}%!`, 'success');
    renderAdminDashboard();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// 2. Admin All Orders View
async function renderAdminOrders() {
  const container = document.getElementById('main-content');
  try {
    const orders = await API.get('/api/orders/all');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">All Platform Transactions</h1>
          <div class="page-subtitle">Full transaction record of all restaurant orders, statuses, and commission calculations</div>
        </div>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Ref</th>
                <th>Date</th>
                <th>Restaurant Buyer</th>
                <th>Supplier Farm</th>
                <th>Item & Qty</th>
                <th>Order Value</th>
                <th>Commission (5%)</th>
                <th>Supplier Net</th>
                <th>Status</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              ${orders.map(o => `
                <tr>
                  <td><strong>${o.orderNumber}</strong></td>
                  <td><small>${API.formatDate(o.submittedAt)}</small></td>
                  <td><strong>${o.restaurantName}</strong><br><small style="color:var(--slate-500);">${o.deliveryCity}</small></td>
                  <td>${o.supplierName}</td>
                  <td>${o.productTitle}<br><small>${o.quantity} ${o.unit}</small></td>
                  <td><strong>${API.formatNaira(o.totalAmount)}</strong></td>
                  <td style="color: #b45309;">${o.commission ? API.formatNaira(o.commission.commissionAmount) : '—'}</td>
                  <td style="color: #166534; font-weight: 600;">${o.commission ? API.formatNaira(o.commission.supplierNetPayout) : '—'}</td>
                  <td><span class="badge badge-${o.status.toLowerCase()}">${o.status}</span></td>
                  <td>
                    <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${o.id}')">Inspect</button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

// 3. Admin Disputes Management
async function renderAdminDisputes() {
  const container = document.getElementById('main-content');
  try {
    const disputes = await API.get('/api/disputes');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Disputes & Complaints Center</h1>
          <div class="page-subtitle">Resolve delivery discrepancies, spoiled goods, or transit damage claims</div>
        </div>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Filed At</th>
                <th>Reason</th>
                <th>Description</th>
                <th>Status</th>
                <th>Resolution Notes</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              ${disputes.length === 0 ? `
                <tr><td colspan="7" class="empty-state">No disputes filed. Platform fulfillment running smoothly!</td></tr>
              ` : disputes.map(d => `
                <tr>
                  <td><strong>${d.orderNumber}</strong></td>
                  <td><small>${API.formatDateTime(d.raisedAt)}</small></td>
                  <td><strong style="color: var(--danger);">${d.reason}</strong></td>
                  <td><small style="max-width: 250px; display: inline-block;">${d.description}</small></td>
                  <td><span class="badge badge-disputed">${d.status}</span></td>
                  <td><small>${d.resolutionNotes || 'Pending review'}</small></td>
                  <td>
                    ${d.status === 'Pending' ? `
                      <button class="btn btn-primary btn-sm" onclick="openResolveDisputeModal('${d.id}', '${d.orderNumber}')">Resolve</button>
                    ` : `
                      <span style="font-size: 0.75rem; color: var(--slate-500); font-weight:600;">Resolved</span>
                    `}
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

function openResolveDisputeModal(disputeId, orderNumber) {
  const contentHtml = `
    <form onsubmit="handleResolveDisputeSubmit(event, '${disputeId}')">
      <div class="form-group">
        <label class="form-label">Resolution Decision</label>
        <select id="resolve-status" class="form-control" required>
          <option value="ResolvedCompleted">Mark Completed & Calculate Commission (Goods accepted / resolved with kitchen)</option>
          <option value="ResolvedRefunded">Cancel Order & Restore Stock (Goods rejected / damaged beyond use)</option>
          <option value="Rejected">Dismiss Dispute & Resume Dispatch Status</option>
        </select>
      </div>
      <div class="form-group">
        <label class="form-label">Resolution Notes</label>
        <textarea id="resolve-notes" class="form-control" rows="3" placeholder="Provide administrator ruling details and audit rationale" required></textarea>
      </div>
      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button type="button" class="btn btn-secondary" onclick="closeAppModal()">Cancel</button>
        <button type="submit" class="btn btn-primary">Apply Resolution</button>
      </div>
    </form>
  `;
  openAppModal(`Resolve Dispute: ${orderNumber}`, contentHtml);
}

async function handleResolveDisputeSubmit(e, disputeId) {
  e.preventDefault();
  const status = document.getElementById('resolve-status').value;
  const resolutionNotes = document.getElementById('resolve-notes').value;

  try {
    await API.post(`/api/disputes/${disputeId}/resolve`, { status, resolutionNotes });
    closeAppModal();
    showToast('Dispute resolved successfully!', 'success');
    renderAdminDisputes();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// 4. Admin Suppliers Management
async function renderAdminSuppliers() {
  const container = document.getElementById('main-content');
  try {
    const suppliers = await API.get('/api/admin/suppliers');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Livestock Suppliers Directory</h1>
          <div class="page-subtitle">Approve or suspend farm suppliers, inspect fulfilled orders, and track commission deductions</div>
        </div>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Farm Business</th>
                <th>Contact</th>
                <th>Location & Coverage</th>
                <th>Active Listings</th>
                <th>Orders Fulfilled</th>
                <th>Gross Sales</th>
                <th>Commission Paid</th>
                <th>Net Earnings</th>
                <th>Approval Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              ${suppliers.map(s => `
                <tr>
                  <td><strong>${s.businessName}</strong></td>
                  <td>${s.contactPerson}<br><small style="color:var(--slate-500);">${s.email} | ${s.phone}</small></td>
                  <td>${s.location}, ${s.state}<br><small style="color:var(--slate-400);">Cov: ${s.deliveryCoverage}</small></td>
                  <td><strong>${s.productCount}</strong></td>
                  <td>${s.totalOrdersFulfilled}</td>
                  <td>${API.formatNaira(s.totalGrossSales)}</td>
                  <td style="color: #b45309;">${API.formatNaira(s.totalCommissionDeducted)}</td>
                  <td style="color: #166534; font-weight: 700;">${API.formatNaira(s.totalNetEarnings)}</td>
                  <td>
                    <span class="badge ${s.isApproved ? 'badge-completed' : 'badge-rejected'}">
                      ${s.isApproved ? 'Approved' : 'Suspended'}
                    </span>
                  </td>
                  <td>
                    <button class="btn ${s.isApproved ? 'btn-danger' : 'btn-success'} btn-sm" onclick="setSupplierApproval('${s.id}', ${!s.isApproved})">
                      ${s.isApproved ? 'Suspend' : 'Approve'}
                    </button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}

async function setSupplierApproval(supplierId, newApprovalStatus) {
  try {
    await API.post(`/api/admin/suppliers/${supplierId}/approval`, { isApproved: newApprovalStatus });
    showToast(`Supplier status updated to: ${newApprovalStatus ? 'Approved' : 'Suspended'}`, 'success');
    renderAdminSuppliers();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// 5. Admin Restaurants Directory
async function renderAdminRestaurants() {
  const container = document.getElementById('main-content');
  try {
    const restaurants = await API.get('/api/admin/restaurants');

    container.innerHTML = `
      <div class="page-header">
        <div>
          <h1 class="page-title">Registered Restaurants Directory</h1>
          <div class="page-subtitle">Commercial restaurants, hotel banquets, and corporate caterers ordering protein</div>
        </div>
      </div>

      <div class="table-card">
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Restaurant Name</th>
                <th>Contact Person</th>
                <th>Email & Phone</th>
                <th>City & Address</th>
                <th>Total Orders</th>
                <th>Total Completed Spend</th>
                <th>Account Status</th>
              </tr>
            </thead>
            <tbody>
              ${restaurants.map(r => `
                <tr>
                  <td><strong>${r.restaurantName}</strong></td>
                  <td>${r.contactPerson}</td>
                  <td>${r.email}<br><small style="color:var(--slate-500);">${r.phone}</small></td>
                  <td>${r.city}, ${r.state}<br><small style="color:var(--slate-400);">${r.address}</small></td>
                  <td><strong>${r.totalOrdersPlaced}</strong></td>
                  <td><strong style="color: var(--primary);">${API.formatNaira(r.totalSpend)}</strong></td>
                  <td>
                    <span class="badge ${r.isActive ? 'badge-completed' : 'badge-rejected'}">
                      ${r.isActive ? 'Active' : 'Deactivated'}
                    </span>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  } catch (err) {
    container.innerHTML = `<div class="empty-state"><div class="empty-desc">${err.message}</div></div>`;
  }
}
