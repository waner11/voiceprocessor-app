#!/usr/bin/env bash

# VoiceProcessor Web - Development Environment Setup Script
# This script checks prerequisites, installs dependencies, and prepares the development environment

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
print_header() {
    echo -e "\n${BLUE}===================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}===================================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Version comparison helper
version_ge() {
    [ "$(printf '%s\n' "$1" "$2" | sort -V | head -n1)" = "$2" ]
}

# Main setup
main() {
    print_header "VoiceProcessor Web - Development Setup"
    
    # Step 1: Check prerequisites
    print_header "Step 1: Checking Prerequisites"
    check_prerequisites
    
    # Step 2: Install dependencies
    print_header "Step 2: Installing Dependencies"
    install_dependencies
    
    # Step 3: Check backend API
    print_header "Step 3: Checking Backend API"
    check_backend_api
    
    # Step 4: Generate API client (if needed)
    print_header "Step 4: API Client Generation"
    generate_api_client
    
    # Step 5: Environment setup
    print_header "Step 5: Environment Configuration"
    setup_environment
    
    # Final summary
    print_header "Setup Complete!"
    print_success "Development environment is ready"
    print_info "Run 'npm run dev' or 'pnpm dev' to start the development server"
    print_info "Visit http://localhost:3000 to view the app"
}

# Check prerequisites
check_prerequisites() {
    local all_good=true
    
    # Check Node.js
    if command_exists node; then
        local node_version=$(node --version | sed 's/v//')
        if version_ge "$node_version" "20.0.0"; then
            print_success "Node.js $node_version (>= 20.0.0 required)"
        else
            print_error "Node.js $node_version is too old (>= 20.0.0 required)"
            all_good=false
        fi
    else
        print_error "Node.js not found (>= 20.0.0 required)"
        print_info "Install from: https://nodejs.org/"
        all_good=false
    fi
    
    # Check package manager (prefer pnpm, fallback to npm)
    if command_exists pnpm; then
        print_success "pnpm $(pnpm --version) found"
        PKG_MANAGER="pnpm"
    elif command_exists npm; then
        print_warning "npm found, but pnpm is recommended"
        print_info "Install pnpm: npm install -g pnpm"
        PKG_MANAGER="npm"
    else
        print_error "No package manager found (npm or pnpm required)"
        all_good=false
    fi
    
    # Check Docker (optional but recommended)
    if command_exists docker; then
        print_success "Docker $(docker --version | awk '{print $3}' | sed 's/,//') found"
        DOCKER_AVAILABLE=true
    else
        print_warning "Docker not found (optional, but recommended for backend)"
        print_info "Install from: https://www.docker.com/get-started"
        DOCKER_AVAILABLE=false
    fi
    
    # Check git
    if command_exists git; then
        print_success "Git $(git --version | awk '{print $3}') found"
    else
        print_error "Git not found"
        all_good=false
    fi
    
    if [ "$all_good" = false ]; then
        print_error "Prerequisites check failed. Please install missing dependencies."
        exit 1
    fi
}

# Install dependencies
install_dependencies() {
    if [ -f "package.json" ]; then
        print_info "Installing npm packages with $PKG_MANAGER..."
        
        if [ "$PKG_MANAGER" = "pnpm" ]; then
            pnpm install
        else
            npm install
        fi
        
        print_success "Dependencies installed"
    else
        print_error "package.json not found. Are you in the project root?"
        exit 1
    fi
}

# Check backend API availability
check_backend_api() {
    # Check if .env.local exists
    if [ -f ".env.local" ]; then
        # Try to extract API URL from .env.local
        API_URL=$(grep NEXT_PUBLIC_API_URL .env.local | cut -d '=' -f2 | tr -d '"' | tr -d "'")
        
        if [ -z "$API_URL" ]; then
            API_URL="http://localhost:5000"
        fi
    else
        API_URL="http://localhost:5000"
    fi
    
    print_info "Checking backend API at $API_URL..."
    
    # Try to ping the API
    if curl -s -o /dev/null -w "%{http_code}" "$API_URL/health" | grep -q "200"; then
        print_success "Backend API is running at $API_URL"
        BACKEND_RUNNING=true
    else
        print_warning "Backend API not reachable at $API_URL"
        print_info "The frontend will work, but API calls will fail"
        print_info "To start the backend:"
        print_info "  1. Clone voiceprocessor-api repository"
        print_info "  2. Run: docker-compose up -d db redis"
        print_info "  3. Run: dotnet run --project src/VoiceProcessor.Clients.Api"
        BACKEND_RUNNING=false
    fi
}

# Generate API client
generate_api_client() {
    # Check if API client generation script exists
    if grep -q "generate:api" package.json; then
        print_info "API client generation script found"
        
        # Check if API client is out of date
        if [ "$BACKEND_RUNNING" = true ]; then
            print_info "Checking if API client needs regeneration..."
            
            # Try to fetch OpenAPI spec
            if curl -s "$API_URL/swagger/v1/swagger.json" -o /tmp/swagger.json 2>/dev/null; then
                # Compare with existing spec (if exists)
                if [ -f "src/lib/api/swagger.json" ]; then
                    if ! diff -q /tmp/swagger.json src/lib/api/swagger.json >/dev/null 2>&1; then
                        print_warning "API spec has changed, regenerating client..."
                        $PKG_MANAGER run generate:api
                        print_success "API client regenerated"
                    else
                        print_success "API client is up to date"
                    fi
                else
                    print_info "Generating API client for the first time..."
                    $PKG_MANAGER run generate:api
                    print_success "API client generated"
                fi
            else
                print_warning "Could not fetch OpenAPI spec from backend"
                print_info "Run '$PKG_MANAGER run generate:api' manually when backend is running"
            fi
        else
            print_warning "Backend not running, skipping API client generation"
            print_info "Run '$PKG_MANAGER run generate:api' after starting the backend"
        fi
    else
        print_info "No API client generation script found (skipping)"
    fi
}

# Setup environment
setup_environment() {
    if [ ! -f ".env.local" ]; then
        if [ -f ".env.example" ]; then
            print_info "Creating .env.local from .env.example..."
            cp .env.example .env.local
            print_success ".env.local created"
            print_warning "Please review .env.local and update values as needed"
        else
            print_warning ".env.example not found"
            print_info "Creating basic .env.local..."
            cat > .env.local << EOF
# VoiceProcessor Web - Environment Variables
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_APP_ENV=development
EOF
            print_success ".env.local created with defaults"
        fi
    else
        print_success ".env.local already exists"
    fi
}

# Run main function
main
