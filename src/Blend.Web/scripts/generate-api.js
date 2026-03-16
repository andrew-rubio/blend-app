#!/usr/bin/env node

const { execSync } = require('child_process')
const fs = require('fs')
const path = require('path')
const https = require('https')
const http = require('http')

const BACKEND_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
const SPEC_URL = `${BACKEND_URL}/swagger/v1/swagger.json`
const OUTPUT_DIR = path.join(__dirname, '../src/lib/api/generated')
const OUTPUT_FILE = path.join(OUTPUT_DIR, 'schema.d.ts')

function checkBackendAvailable() {
  return new Promise((resolve) => {
    const url = new URL(SPEC_URL)
    const client = url.protocol === 'https:' ? https : http
    
    const req = client.get(SPEC_URL, { timeout: 3000 }, (res) => {
      resolve(res.statusCode === 200)
    })
    
    req.on('error', () => resolve(false))
    req.on('timeout', () => {
      req.destroy()
      resolve(false)
    })
  })
}

async function main() {
  console.log(`Checking backend availability at ${SPEC_URL}...`)
  
  const isAvailable = await checkBackendAvailable()
  
  if (!isAvailable) {
    console.warn(`⚠️  Backend not available at ${SPEC_URL}`)
    console.warn('   Skipping API generation. Run this script when the backend is running.')
    console.warn('   To generate: NEXT_PUBLIC_API_URL=http://localhost:5000 npm run generate-api')
    
    if (!fs.existsSync(OUTPUT_DIR)) {
      fs.mkdirSync(OUTPUT_DIR, { recursive: true })
    }
    
    process.exit(0)
  }
  
  console.log('Backend is available! Generating API types...')
  
  if (!fs.existsSync(OUTPUT_DIR)) {
    fs.mkdirSync(OUTPUT_DIR, { recursive: true })
  }
  
  try {
    execSync(
      `npx openapi-typescript ${SPEC_URL} --output ${OUTPUT_FILE}`,
      { stdio: 'inherit' }
    )
    console.log(`✅ API types generated at ${OUTPUT_FILE}`)
  } catch (error) {
    console.error('❌ Failed to generate API types:', error.message)
    process.exit(1)
  }
}

main()
