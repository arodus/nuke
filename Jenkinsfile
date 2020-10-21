// ------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated.
//
//     - To turn off auto-generation set:
//
//         [Jenkins (AutoGenerate = false)]
//
//     - To trigger manual generation invoke:
//
//         nuke --generate-configuration Jenkins --host Jenkins
//
// </auto-generated>
// ------------------------------------------------------------------------------

pipeline {
  agent { label 'sad' }
  parameters {
    choice(
      name: 'Verbosity',
      defaultValue: 'Normal',
      choices: ['Minimal', 'Normal', 'Quiet', 'Verbose'],
      description: 'Logging verbosity during build execution. Default is \'Normal\'.'
    )
    string(name: 'TwitterConsumerKey')
    string(name: 'TwitterConsumerSecret')
    string(name: 'TwitterAccessToken')
    string(name: 'TwitterAccessTokenSecret')
    string(name: 'SlackWebhook')
    string(name: 'GitterAuthToken')
    string(name: 'NuGetApiKey')
    choice(
      name: 'Configuration',
      defaultValue: 'Release',
      choices: ['Debug', 'Release'],
      description: 'Configuration to build - Default is \'Debug\' (local) or \'Release\' (server)'
    )
    string(name: 'GitHubToken')
    booleanParam(name: 'IgnoreFailedSources', defaultValue: False)
    booleanParam(name: 'AutoStash', defaultValue: True)
    booleanParam(name: 'UseHttps', defaultValue: False)
    string(name: 'SignPathApiToken')
    string(name: 'SignPathOrganizationId')
    string(name: 'SignPathProjectKey')
    string(name: 'SignPathPolicyKey')
  } // parameters
  environment {
    IS_UNIX = isUnix()
    SlackWebhook = credentials('SlackWebhook')
    GitterAuthToken = credentials('GitterAuthToken')
    NuGetApiKey = credentials('NuGetApiKey')
  } // environment
  stages {
    stage('Compile') {
      steps {
        nuke 'Restore Compile --skip'
      } // steps
    } // stage('Compile')
    stage('Pack') {
      steps {
        nuke 'Pack --skip'
      } // steps
      post {
        success {
          archiveArtifacts('output/packages/*.nupkg')
        } // success
      } // post
    } // stage('Pack')
    stage('Test') {
      steps {
        nuke 'Test --skip'
      } // steps
      post {
        success {
          archiveArtifacts('output/test-results/*.trx,output/test-results/*.xml')
        } // success
      } // post
    } // stage('Test')
    stage('Coverage') {
      steps {
        nuke 'Coverage --skip'
      } // steps
      post {
        success {
          archiveArtifacts('output/coverage-report.zip')
        } // success
      } // post
    } // stage('Coverage')
  } // stages
} // pipeline

void nuke(String args) {
  if (Boolean.valueOf(env.IS_UNIX)) {
    sh ("build.cmd $args")
  }
  else {
    powershell ("build.cmd $args")
  }
}
